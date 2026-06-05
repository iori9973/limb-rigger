# Changelog

バージョン番号は [Semantic Versioning](https://semver.org/lang/ja/) に従います。

- **MAJOR**: 後方互換性のない変更
- **MINOR**: 後方互換性のある機能追加
- **PATCH**: 後方互換性のあるバグ修正

---

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
