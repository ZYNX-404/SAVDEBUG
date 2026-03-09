# SaccAirVehicle Engine ON→OFF / EngineOutput=0 Static Path Analysis

対象: `SaccAirVehicle` + `DFUNC_ToggleEngine` + `SaccEntity` + `SaccVehicleSeat`

## 0) 今回の再検証で固定する前提（更新）

- AI が参照している `SaccAirVehicle` と実体の `SaccAirVehicle` は同一 GameObject。
- ただし **ClientSim 実行時は Mono 側 `SaccAirVehicle` フィールド状態と Udon 実行状態が乖離し得る**。
- 実際に `DFUNC_ToggleEngine.JustEngineOn -> SaccAirVehicle.EngineOn` 到達中でも、Mono 側 readiness 条件（`VehicleAnimator` / `VehicleRigidbody` / `IsOwner` など）が長時間不正に見える run がある。
- `EngineOn` setter 内で `VehicleAnimator.SetBool("EngineOn", true)` 近傍の `NullReferenceException` を観測しており、Mono 側準備完了判定を制御ゲートに使うのは不安定。
- したがって「即OFF系（維持失敗）」と「EngineOutput=0系（入力封鎖）」に加え、**ClientSim 由来の Mono/Udon 状態不一致**を独立原因として扱う。

## 1) EngineOff に到達する全経路（SaccAirVehicle系）

### A. 直接 `SetEngineOff()` を呼ぶ経路
1. `SFEXT_G_Wrecked()` で即 `SetEngineOff()`。
2. `Explode()` で即 `SetEngineOff()`。
3. `SFEXT_G_RespawnButton()` で `_EngineOn` が true のとき `SetEngineOff()`。

### B. `SendCustomNetworkEvent(All, nameof(SetEngineOff))` 経由
1. `SFEXT_O_TakeOwnership()` の else 分岐（`_EngineOn` true かつ `((EntityControl.Piloting || !Occupied) && !pilotLeftFlag)` を満たさない）で EngineOff を全員へ送信。
2. `SFEXT_O_PilotExit()` で `EngineOffOnExit && !_PreventEngineToggle` のとき EngineOff を全員へ送信。
3. `SendNoFuel()` で `IsOwner` のとき EngineOff を全員へ送信。
4. `SFEXT_L_SetEngineOff()`（外部拡張が呼べるローカル入口）で EngineOff を全員へ送信。

### C. `_EngineOn=false` 代入（DFUNC側）経由
1. `DFUNC_ToggleEngine.EngineOff()` が `SAVControl.SetProgramVariable("_EngineOn", false)` を実行。
   - 条件: `SAVControl._EngineOn == true` かつ `SAVControl._PreventEngineToggle == false`。
2. これはネットワークイベント `ToggleEngine()->EngineOff()` からも到達。

## 2) 優先調査ポイントの結論

### `SFEXT_O_TakeOwnership()`
- 最重要。`_EngineOn` true の瞬間に ownership 取得が走り、判定条件を外すと **即 EngineOff を全員送信** する。
- 判定キー:
  - `EntityControl.Piloting`
  - `Occupied`
  - `EntityControl.pilotLeftFlag`
- コメントで `OnPlayerLeft()` と `OnOwnershipTransferred()` の順序がランダムである旨が明記されており、初回再現性の低い競合と整合。

### `SFEXT_O_PilotExit()`
- `EngineOffOnExit` が true（デフォルト）なら、離席イベント経由で EngineOff される。
- Seat 系 (`SaccVehicleSeat`) では `OnStationExited` / `OnPlayerLeft` から `PilotExitVehicle()` が通り、`SFEXT_O_PilotExit` が発火し得る。

### `DFUNC_ToggleEngine.EngineOff()`
- `ToggleMinDelay` 後に明示的なトグル操作や double tap 判定で EngineOff 実行。
- ただし今回の「EngineOn直後 0.02〜0.05s で落ちる」現象は、`ToggleMinDelay` の存在上、通常操作起因より ownership/seat イベント起因の方が整合しやすい。

### `SetProgramVariable("_EngineOn", false)`
- 現在コード上の直接代入は `DFUNC_ToggleEngine.EngineOff()` のみ。

### `SetEngineOff()` への到達まとめ
- 破損・爆発・リスポーン・オーナー移行・離席・燃料切れ・外部拡張イベント・DFUNCトグル。

## 3) EngineOn=true でも EngineOutput=0.00 になりうる条件

### A. `ThrottleInput` 側が 0 のまま
- `FixedUpdate` の EngineOutput 更新は `_DisablePhysicsAndInputs == false` ブロック内のみ。
- `if (_EngineOn) EngineOutput -> ThrottleInput` のため、`ThrottleInput==0` なら EngineOutput は 0 近傍に留まる。

### B. `_DisablePhysicsAndInputs` が true
- EngineOutput 更新分岐そのものに入らない（固定更新内の入力/出力 lerp をスキップ）。
- 結果として EngineOutput が既存値（初期 0）から動かない。

### C. `_DisableThrottleControl` が true
- 通常の操縦入力で `ThrottleInput` を上げる経路が封じられる。
- `_ThrottleOverridden` で上書きしない限り 0 維持しやすい。

### D. `Fuel`/`NoFuel`
- `FuelEvents()` で燃料不足時に `ThrottleInput` が `Fuel * LowFuelDivider` へ制限。
- `Fuel==0` では `SendNoFuel()` が EngineOff を送信。

### E. 所有者条件
- EngineOutput の lerp 更新は owner 側の `FixedUpdate` で実施。ownership 遷移直後の状態持ち越し/ゼロ初期値の瞬間は発生し得る。

### F. 拡張による入力封鎖
- `SAV_ThreeDThrust` は設定次第で `DisableThrottleControl` を `+1` する。
- 本ファイル上、対応する `-1` が見当たらず、構成次第でスロットル入力無効が残留する可能性がある。

## 4) 「初回だけ失敗しやすい」要因（競合）

1. **Ownership vs PlayerLeft の順序競合**
   - `SaccAirVehicle` コメントに順不同明記。
   - `pilotLeftFlag` は `SaccVehicleSeat.OnPlayerLeft()` で立ち、1フレーム後に戻す。
   - この短時間窓で `SFEXT_O_TakeOwnership` 判定が割れる。

2. **Seat Exit / PilotExit 連鎖の同フレーム競合**
   - `OnStationExited -> PlayerExitPlane -> PilotExitVehicle -> SFEXT_O_PilotExit`。
   - `EngineOffOnExit` true だと EngineOn 直後でも離席系イベントで即オフ化。

3. **遅延イベント（EngineStartupFinish）と状態変化競合**
   - `DFUNC_ToggleEngine` は `SendCustomEventDelayedSeconds(EngineStartupFinish)` を使用。
   - delayed 完了時に owner/dead/fuel/prevent 状態が変化していると、初回だけ不一致が起こりやすい。

4. **生成物ノイズ（Udon asset 再生成）**
   - 実行対象が同じ C# でも生成済み Udon program の更新タイミング差で再現性が揺れる。

## 5) 最も怪しい3経路（優先度付き）

1. **P1: `SFEXT_O_TakeOwnership` の EngineOff 分岐**
   - 根拠: EngineOn 直後短時間で OFF になる現象と、所有権移行イベントの非決定順序コメントが一致。

2. **P2: `SFEXT_O_PilotExit`（Seat由来）**
   - 根拠: Station/Seat 系イベント一発で `SetEngineOff(All)` へ到達。RWS/座席の初期アタッチ・離席誤判定で発火し得る。

3. **P3: `_DisableThrottleControl` 残留（ThreeDThrust含む）による `EngineOutput=0` 固定**
   - 根拠: EngineOn維持でも出力0の説明に最短で一致。入力系を止めると `ThrottleInput` が上がらず、EngineOutput lerp も0維持。

---

## 追加の最小ログ提案（次回検証）
- `SFEXT_O_TakeOwnership` 直前直後で `Piloting/Occupied/pilotLeftFlag/Using` を1行ログ。
- `SFEXT_O_PilotExit` 入口で `EngineOffOnExit/_PreventEngineToggle` を1行ログ。
- `FixedUpdate` EngineOutput更新直前で `_DisablePhysicsAndInputs/_DisableThrottleControl/_ThrottleOverridden/ThrottleInput/EngineOutput` を1行ログ。


## 6) 再検証チェックリスト（即OFF系と出力0系を分離）

### 6-A. 即OFF系（ON→0.02〜0.05sでOFF）
1. `SFEXT_O_TakeOwnership` の条件ログを優先採取
   - `_EngineOn / EntityControl.Piloting / Occupied / pilotLeftFlag / Using`
2. 同フレーム近傍で `SFEXT_O_PilotExit` 発火有無を採取
   - `EngineOffOnExit / _PreventEngineToggle`
3. `SendNoFuel()` 発火有無を採取
   - 燃料由来 OFF を除外

### 6-B. EngineOn維持だが EngineOutput=0 系
1. `FixedUpdate` で `_DisablePhysicsAndInputs` が 0 か確認
2. `_DisableThrottleControl` と `_ThrottleOverridden` の組み合わせ確認
3. `ThrottleInput / PlayerThrottle / EngineOutput` の三点同時ログ
4. owner 側インスタンスで観測しているか確認（非owner値読み違い除外）

### 6-C. 判定
- **即OFF系が再現**: `TakeOwnership` or `PilotExit` の event race を第一候補。
- **即OFFせず出力0のみ再現**: 入力封鎖 (`_DisableThrottleControl` 等) を第一候補。
- 両方再現する run がある場合は、別要因が同時に存在する前提でログを分離して評価する。


## 7) ClientSim runtime 起因の状態乖離（新規）

### 観測
- 同一機体オブジェクト上で `SaccAirVehicle` / `UdonBehaviour` / `ClientSimUdonHelper` が共存する PlayMode run で、
  Mono 側 AI が正しい機体を掴んでいても、以下 readiness が長時間 invalid のままになることがある。
  - `sav.VehicleAnimator`
  - `sav.VehicleRigidbody`
  - `sav.IsOwner`
  - `sav.EntityControl.IsOwner`
- その最中に delayed `JustEngineOn` は到達し、`SaccAirVehicle.EngineOn` には入る。
- さらに `EngineOn` setter の `VehicleAnimator.SetBool(...)` 近傍で `NullReferenceException` が発生し得る。

### 解釈
- ClientSim の helper 追加・UdonManager 初期化順により、Mono 側が読むフィールドと Udon 実行コンテキストの時系列が一致しない。
- よって、Mono 側ポーリングに依存した「Udon準備完了ゲート」は誤判定を起こす。

### 推奨修正方針
1. Mono readiness gate から以下を外す。
   - `VehicleAnimator`
   - `VehicleRigidbody`
   - `IsOwner`
   - `EntityControl.IsOwner`
2. ゲートは構造的条件（対象オブジェクト同一性など）に最小化する。
3. 機体制御ブリッジは Mono 直読より Udon/event-driven 呼び出しへ寄せる。
4. 必要なら「Udon側が準備完了したこと」を明示通知するシグナル（カスタムイベント/フラグ）を設ける。

### なぜ重要か
- 症状は「EngineOn 後すぐ失敗」「AI が ready にならない」に見えるが、根因が単純な機体ロジックでなく
  ClientSim + Mono + Udon の状態同期ギャップである可能性が高い。

