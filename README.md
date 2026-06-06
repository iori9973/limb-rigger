# Limb Rigger

VRChat アバターにサブアーム・サブレッグ・義手・義足などの追加パーツを取り付け、本体のボーンに `VRCRotationConstraint` で追従させるセットアップを自動化する Unity Editor 拡張です。

## 動作環境

- Unity 2022.3.22f1
- VRChat SDK (com.vrchat.avatars) 3.7.0 以上
- Windows / macOS / Linux

## インストール

### VCC（VRChat Creator Companion）からインストール（推奨）

以下のページを開き、**Add to VCC** ボタンをクリックしてください。

https://iori9973.github.io/limb-rigger/

ボタンが動作しない場合は、VCC の **Settings → Packages → Add Repository** に以下の URL を直接貼り付けてください。

```
https://iori9973.github.io/limb-rigger/index.json
```

追加後は **My Projects → Manage Project → Limb Rigger → Install** でインストールできます。

### Package Manager からインストール

1. Unity の Package Manager を開く
2. **Add package from disk...** を選択
3. `limb-rigger/package.json` を選択

---

## 使い方

メニューから **Tools → Limb Rigger** を開きます。雪空からす氏が紹介していた「RotationConstraint によるサブアーム実装手順」を 1 ウィンドウ + 3 ボタンに自動化したツールです。

### 基本フロー (4 ステップ)

1. **シーンに本体アバターとサブパーツを配置**
   - サブパーツの位置・スケールを Scene View で目視合わせ (アーマチュア接続前に必須)
2. **Limb Rigger ウィンドウでフィールド入力**
   - `Avatar Root`: 本体アバター (Humanoid Animator がある GameObject)
   - `Attachment Point`: 取り付け先の本体ボーン (例: `Chest` `Hips`)
   - `Sub Limb Root`: 取り付けるサブパーツのルート Transform
   - (フルアバターから一部だけ流用する場合のみ) Advanced > `Wrapper Root` にサブパーツ全体の prefab ルート、`Sub Limb Root` には取り出したいサブツリーのルートを指定
3. **「アーマチュア接続」→「マッピング解析」→ プレビュー確認**
   - 自動マッピング結果は色分けされる: 緑 (Humanoid) / 黄 (NameMatch) / 青 (ManualOverride) / 赤 (Unmapped)
   - 想定外のマッピングは右側 ObjectField で手動上書き
4. **「適用」ボタンで VRCRotationConstraint を一括生成**
   - すべての操作は Ctrl+Z 1 回でロールバック可能
   - やり直したいときは「生成物を削除」ボタン

### ユースケース別の推奨指定

`Attachment Point` と `Sub Limb Root` はやりたいことで変わります。

| やりたいこと | Attachment Point | Sub Limb Root |
|---|---|---|
| サブアーム化 (例: 4 本腕化) | `Chest` | サブアーム prefab のルート |
| 片腕の義手化 | `LeftLowerArm` または `RightLowerArm` | 義手 prefab のルート |
| サブレッグ | `Hips` | サブレッグ prefab のルート |
| 義足化 | `LeftLowerLeg` または `RightLowerLeg` | 義足 prefab のルート |
| 頭部置換 | `Neck` | 頭パーツ prefab のルート |
| 手持ち武器・杖 | `LeftHand` または `RightHand` | アクセサリ prefab のルート |
| フルアバターから一部だけ流用 | 部位に応じた本体ボーン (左腕なら `Chest`) | サブアバター内の部位サブツリーのルート (例: Anubis の `Shoulder.L`)。**Advanced > Wrapper Root** にサブアバター全体の prefab ルートを指定 |

注意: `Avatar Root` や `Armature` のような上位 GameObject を Attachment Point に指定すると、体の動きに追従しない不自然な挙動になります。Limb Rigger ウィンドウ内でも警告 HelpBox が表示されます。

### Modular Avatar との連携

Limb Rigger は **Constraint 生成までを責務**としており、以下の作業は範囲外です:

- 本体側の対応部位 (元の腕など) の非表示
- サブパーツの ON/OFF 切替トグル
- アバター全体の Hierarchy 管理

これらは [Modular Avatar](https://modular-avatar.nadena.dev/) の以下機能で実現してください:

- **`MA Object Toggle` + `MA Menu Item`**: Expression Menu からサブパーツを ON/OFF
- **`MA Mesh Settings`**: ビルド時にメッシュの可視性を切替
- **`MA Merge Animator`** など: Animator 統合

Limb Rigger の「適用」ボタン押下後にも案内 HelpBox が表示されます。

### 自動マッピングの対応範囲

ベースアバターが Humanoid 設定済みであれば、以下の命名パターンを自動で吸収します。

| 命名規則 | 例 | 対応 |
|---|---|---|
| Unity Humanoid 標準 | `LeftUpperArm` / `RightHand` | ✓ |
| `_ . - 半角空白` 区切り | `Upper_arm.L` `Lower Arm.L` `Hand-R` | ✓ |
| Blender 重複サフィックス | `Hand.L.001` `Arm.L.002` | ✓ |
| Mixamo | `mixamorig:LeftHandIndex1` `LeftUpLeg` `Pinky` | ✓ |
| VRoid Studio | `J_Bip_L_UpperArm` `J_Bip_C_Neck` `J_Sec_*` | ✓ |
| 3DS Max biped | `Bip01_L_Hand` `Bip001_L_Forearm` | ✓ |
| 上腕/前腕の同義語 | `UpperArm↔Arm` `ForeArm↔LowerArm` | ✓ |
| 脚の同義語 | `Thigh↔UpperLeg` `Calf↔Shin↔LowerLeg` | ✓ |
| 指関節の同義語 | `Proximal/Intermediate/Distal/Metacarpal` 短縮 | ✓ |
| 日本語ボーン名 | `腕.L` `手.L` | ✗ (Tier 3 手動オーバーライドで対応) |

### 動作確認済みの組み合わせ

- ベース: BeroarN `bn0010_Marycia` (Marycia / Humanoid)
- サブアーム: `Titan.V2 / Titan2arm` (Generic Avatar)
- 上記の組合せで 28 ボーンが自動マッピング、VRChat 内で `Hand Gesture` 連動の指追従までを確認済み

### 既知の制限

- ベースアバターは Humanoid 必須 (Generic ベースは Tier 2 名前マッチのみで非推奨)
- 自動マッピングが赤 (Unmapped) のままの場合は Tier 3 手動オーバーライドで対応
- Edit モードでの動作プレビューは VRChat SDK の Constraint Preview 設定に依存 (Project Settings → VRChat SDK → Constraints)
- サブアーム/義手の表示オン/オフ切替 (Modular Avatar 統合) は未実装 — 次バージョン以降の予定

---

## ライセンス

MIT License

## サポート

X（Twitter）: https://x.com/iori__9973
