# Changelog

バージョン番号は [Semantic Versioning](https://semver.org/lang/ja/) に従います。

- **MAJOR**: 後方互換性のない変更
- **MINOR**: 後方互換性のある機能追加
- **PATCH**: 後方互換性のあるバグ修正

---

## [0.3.0] - 2026-06-29

### Added
- **マージ方式の適用ボタンを追加**。各サブパーツボーンを対応する本体ボーンの子へ再ペアレントし、位置・回転ごと本体に追従させる。Rotation Constraint と違い、適用時の配置をそのまま保持するため、義手・義足など「本体に固定して動かす」用途でずれない。VRChat のコンストレイント内部計算に依存しない確実な方式

### Changed
- Step 3「Apply」に「マージ方式 (推奨)」と「Constraint 方式 (回転のみ)」の2ボタンを用意し、用途の違いを HelpBox で明示

### Note
- Rotation Constraint 方式は本体とサブパーツのボーン長が異なると位置がずれる制約があり、配置保持が必要な用途ではマージ方式を推奨

## [0.2.5] - 2026-06-28

### Fixed
- Constraint 適用時にサブパーツのボーンが本体ボーンの絶対回転にスナップし、義手などがずれて見える問題を修正。適用時点の向きを維持する `ParentRotationOffset`（rest オフセット）を焼き込み、`RotationAtRest` も現在のローカル回転に設定するようにした

## [0.2.4] - 2026-06-06

### Added
- `docs/index.html` を追加。https://iori9973.github.io/limb-rigger/ にアクセスすると VCC へリポジトリを追加する「Add to VCC」ボタン (`vcc://vpm/addRepo?url=...`) と手動 URL コピーペースト案内を含む landing page が表示される
- README にユースケース別の推奨指定表 (サブアーム / 義手 / 義足 / 頭部置換 / フルアバター流用ごとの Attachment Point と Sub Limb Root の指定例) を追加
- README に Modular Avatar との連携セクションを追加 (MA Object Toggle / MA Mesh Settings / MA Merge Animator 等の使い分け案内)

## [0.2.3] - 2026-06-06

### Added
- 「アーマチュア接続」「適用」「生成物を削除」ボタンを押した直後に、`EditorUtility.DisplayDialog` で結果ポップアップを表示。何件のボーンを処理したか・次に何をすればよいか・取り消し方法を明示。Console ログだけだと処理が完了したか分かりづらく不安だった問題を解消

## [0.2.2] - 2026-06-06

### Added
- Apply ボタンの直後に Modular Avatar 案内 HelpBox を追加。本体側元部位の非表示やサブパーツの On/Off 切替は MA Object Toggle / MA Mesh Settings 等で行ってもらうように責務分離を明示

## [0.2.1] - 2026-06-06

### Added
- Attachment Point の選択ガイダンスをツールチップに記載 (サブアーム→Chest、サブレッグ→Hips、義手→LeftLowerArm 等の推奨)
- Attachment Point に Avatar Root や Humanoid Hips より上の階層 (Armature 等) を指定した場合に HelpBox で警告

## [0.2.0] - 2026-06-06

初回リリース可能版。

### Added
- `BoneMapper` Tier 1.5 マッピングを追加 — サブ側 Animator が非 Humanoid (義手単体パーツなど) でも、ベース側が Humanoid なら `HumanBodyBones` 列挙からサブ側を名前正規化マッチで自動検出
- `BoneMapper.Normalize` の対応命名パターンを大幅に拡張:
  - Marycia / Anubis 系の `_ . - 半角空白` 区切り + `*Left` / `*Right` ↔ `*.L` / `*.R` の表記揺れ吸収
  - 末尾の Blender 重複サフィックス (`.001` 等) を除去
  - セマンティックエイリアス: `UpperArm↔Arm` `ForeArm↔LowerArm` `Thigh↔UpperLeg` `Calf↔LowerLeg` `Clavicle↔Shoulder` `Wrist↔Hand` `Ankle↔Foot`
  - 指のボーン: `Proximal/Intermediate/Distal/Metacarpal` を短縮形に正規化
  - Mixamo 系: `mixamorig:LeftHandIndex1` 等のプレフィックス除去 + 指の番号 (`Index1/2/3` → `Proximal/Intermediate/Distal`) 変換 + `Pinky` を `Little` に同一視 + `UpLeg → UpperLeg`
  - VRoid Studio 系: `J_Bip_`, `J_Sec_` プレフィックス除去 + 単独 `L_`/`R_` を `Left_`/`Right_` 化 + `C_` 中央プレフィックス除去
  - 3DS Max biped 系: `Bip01_`, `Bip001_` プレフィックス除去
  - 汎用: `Bone_`, `Armature|` プレフィックス除去
- `LimbRiggerWindow` の入力フィールドを整理。「Wrapper Root」は Foldout 配下の Advanced 項目に格下げし、単体パーツ利用時は `Sub Limb Root` 1 つで完結
- 操作順を示すヘルプボックスと、マッピング解析時の Tier 別集計 (H / N / M / Unmapped 件数) を表示
- Avatar Root の Animator が無い / Humanoid 未設定の際にエラー HelpBox を表示
- `Tests/Editor/BoneMapperNormalizeTests.cs` EditMode テスト追加。Marycia / Titan2arm / Anubis / Mixamo / VRoid / 3DS Max の各命名パターンへの対応をリグレッション防止

### Changed
- 生成する Constraint を Unity 標準 `RotationConstraint` から VRChat 公式の `VRCRotationConstraint` (`VRC.SDK3.Dynamics.Constraint.Components`) に変更。Job System ベースで動作が軽く、Avatar Performance Rank への影響も小さい
- `com.vrchat.avatars >= 3.7.0` を VPM 依存に追加 (VRC Constraint 初出バージョン)
- `LimbRigger.Editor.asmdef` に `VRC.SDK3A` / `VRC.SDK3.Dynamics.Constraint` / `VRC.Dynamics` の Assembly Reference を追加
- `ArmatureAttacher.Attach` に「`Sub Limb Root` = `Subtree Root` のとき直接 `Attachment Point` 配下に reparent」する分岐を追加。義手単体パーツが本体の取り付け先に配置されないバグを解消
- `ConstraintApplier.Apply` 時に既存の Unity 標準 `RotationConstraint` を自動削除するように変更。0.1.0 → 0.2.0 移行時の二重 Constraint を解消
- `ConstraintApplier.Remove` で VRC Constraint だけでなく Unity 標準 `RotationConstraint` も削除するように変更

### Fixed
- `BoneMapper` で Attach 後の解析時に、サブ側 `FindParentAnimator` がベース側 Animator を拾い Tier 1 マッピングが破綻するバグを修正。サブ Animator がベース Animator と同一の場合は Tier 1 (Humanoid 直接) をスキップし、Tier 1.5 (名前正規化) フォールバックに切り替え
- `ConstraintApplier.Apply` で `Locked = true` を設定していたため、VRChat SDK の Performance Stats 計算 (`AvatarPerformanceStats.CalculatePerformanceRating`) で NRE が発生してアップロード不可になるバグを修正 (`Locked = false` に変更)
- `ConstraintApplier.Apply` で `RotationAtRest` を設定していたが `IsActive = true` 時には参照されないため削除 (NRE の原因候補を排除)

### Notes
- Edit モードでの動作プレビューは VRChat SDK の Constraint Preview 設定に依存する (Project Settings → VRChat SDK → Constraints)。確実な検証は Play モードまたは VRChat 実機

## [0.1.0] - 2026-05-27

### Added
- プロジェクト初期化
