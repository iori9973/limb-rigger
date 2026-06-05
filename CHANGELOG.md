# Changelog

バージョン番号は [Semantic Versioning](https://semver.org/lang/ja/) に従います。

- **MAJOR**: 後方互換性のない変更
- **MINOR**: 後方互換性のある機能追加
- **PATCH**: 後方互換性のあるバグ修正

---

## [Unreleased]

### Breaking
- 生成する Constraint を Unity 標準 `RotationConstraint` から VRChat 公式の `VRCRotationConstraint` (`VRC.SDK3.Dynamics.Constraint.Components`) に変更。Job System ベースで動作が軽く、Avatar Performance Rank への影響も小さい
- `com.vrchat.avatars >= 3.7.0` を VPM 依存に追加 (VRC Constraint 初出バージョン)
- `LimbRigger.Editor.asmdef` に `VRC.SDK3A` / `VRC.SDK3.Dynamics.Constraint` / `VRC.Dynamics` の Assembly Reference を追加
- Edit モードでの動作プレビューは VRChat SDK の Constraint Preview 設定に依存するようになる (Project Settings → VRChat SDK → Constraints)

### Fixed
- `BoneMapper` で Attach 後の解析時に、サブ側 `FindParentAnimator` がベース側 Animator を拾い Tier 1 マッピングが破綻するバグを修正。サブ Animator がベース Animator と同一の場合は Tier 1 (Humanoid 直接) をスキップし、Tier 1.5 (名前正規化) フォールバックに切り替え

### Changed
- `BoneMapper.Normalize` を強化: 末尾の Blender 重複サフィックス (`.001` 等) を除去、`Left*` / `*Left` ↔ `*.L` の表記揺れを双方向に吸収、`UpperArm↔Arm` `ForeArm↔LowerArm` `Thigh↔UpperLeg` `Calf↔LowerLeg` `Clavicle↔Shoulder` `Wrist↔Hand` `Ankle↔Foot` 等のセマンティック・エイリアスを適用。指のボーン (`Proximal` `Intermediate` `Distal` `Metacarpal`) も短縮形に正規化
- `BoneMapper` の Tier 1 を拡張: サブ側 Animator が非 Humanoid (義手単体パーツなど) でも、ベース側が Humanoid なら `HumanBodyBones` 列挙からサブ側を名前正規化マッチで自動検出
- `ArmatureAttacher.Attach` に「`Sub Limb Root` = `Subtree Root` のとき直接 `Attachment Point` 配下に reparent」する分岐を追加。義手単体パーツが本体の取り付け先に配置されないバグを解消
- `LimbRiggerWindow` の入力フィールドを整理。「Wrapper Root」は Foldout 配下の Advanced 項目に格下げし、単体パーツ利用時は `Sub Limb Root` 1 つで完結
- マッピング解析時のヘッダーに Tier 別の集計 (H / N / M / Unmapped 件数) を表示
- Avatar Root に Animator が無い / マッピング 0 件のケースで警告ログを出力

## [0.1.0] - 2026-05-27

### Added
- プロジェクト初期化
