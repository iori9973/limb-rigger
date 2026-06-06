# Limb Rigger

VRChat アバターに **追加の腕や脚** (サブアーム・義手・義足・頭部置換など) を取り付ける Unity Editor 拡張です。

サブパーツのボーンを本体アバターのボーンに自動マッピングし、`VRCRotationConstraint` を一括設定することで、追加パーツが本体の動きに自然に追従するセットアップを **3 ボタンで完了** できます。

## できること

- カイリキーのような **4 本腕化** (サブアーム)
- 片腕を別の腕に置き換える **義手化**
- **サブレッグ / 義足** の取り付け
- 別アバターの一部 (頭・腕など) を **流用**
- 手持ち武器・杖などの装備品の追従

## 動作環境

- Unity 2022.3
- VRChat SDK (`com.vrchat.avatars`) 3.7.0 以上

---

## インストール

### 1. VCC にリポジトリを追加

[**▶ Add to VCC**](https://iori9973.github.io/limb-rigger/) からワンクリック追加できます。

ボタンが反応しない場合は、VCC の **Settings → Packages → Add Repository** に以下の URL を貼り付けてください。

```
https://iori9973.github.io/limb-rigger/index.json
```

### 2. プロジェクトに追加

VCC で対象プロジェクトを開き、**Manage Project → Packages → Limb Rigger → Install** をクリック。

Unity でプロジェクトを開くと、メニューに `Tools → Limb Rigger` が追加されます。

---

## クイックスタート — サブアーム化を 5 分で

最初のユースケースとして **「本体アバターにサブアームを 2 本追加して 4 本腕にする」** 流れを通して説明します。

### Step 1: シーンに配置する

1. 本体アバター (Humanoid) のプレハブをシーンに配置
2. サブアームのプレハブを**本体の隣**に配置
3. Scene View でサブアームの**位置・スケール**を本体に合わせて目視で調整

> **重要**: アーマチュア接続前に位置とスケールを合わせてください。接続後に動かすと Constraint の rest pose がズレます。

### Step 2: Limb Rigger ウィンドウを開く

メニュー `Tools → Limb Rigger` をクリック。以下を入力します。

| フィールド | 指定するもの |
|---|---|
| **Avatar Root** | 本体アバターのルート GameObject |
| **Attachment Point** | 本体の `Chest` ボーン |
| **Sub Limb Root** | サブアームのルート (prefab のルート GameObject) |

### Step 3: 3 ボタンを順に押す

```
①「アーマチュア接続」   サブアームを本体の Chest 配下に移動
              ↓
②「マッピング解析」     ボーン対応を自動解析、色分けプレビュー表示
              ↓
③ プレビューを確認       (緑=自動マッピング成功、赤=未対応、必要なら手動上書き)
              ↓
④「適用」               VRCRotationConstraint を一括生成
```

各ボタンを押すと **「N 件のボーンを処理しました」というポップアップ** が表示されます。

### Step 4: 確認・完成

- VRChat SDK Control Panel で Build & Test → 実機で本体の腕の動きにサブアームが追従するか確認
- 問題があれば **Ctrl + Z** で一発巻き戻し、または「生成物を削除」ボタンで一括クリア

---

## ユースケース別の指定例

サブアーム以外の用途では、`Attachment Point` と `Sub Limb Root` を以下のように指定します。

| やりたいこと | Attachment Point | Sub Limb Root |
|---|---|---|
| サブアーム化 (4 本腕) | `Chest` | サブアーム prefab のルート |
| 片腕の義手化 | `LeftLowerArm` / `RightLowerArm` | 義手 prefab のルート |
| サブレッグ | `Hips` | サブレッグ prefab のルート |
| 義足化 | `LeftLowerLeg` / `RightLowerLeg` | 義足 prefab のルート |
| 頭部置換 | `Neck` | 頭パーツ prefab のルート |
| 手持ち武器・杖 | `LeftHand` / `RightHand` | アクセサリ prefab のルート |

> **NG な指定**: `Avatar Root` や `Armature` などの上位 GameObject を Attachment Point にすると、サブパーツが体の動きに追従しません。Limb Rigger 内でも警告 HelpBox が表示されます。

---

## 応用: 別アバターの一部だけを流用する

「Anubis の左腕だけを Marycia に取り付ける」のように、別の**フルアバターから一部のサブツリーだけを取り出して**使うこともできます。

このときは Advanced foldout の **Wrapper Root** を併用します。

| フィールド | 指定するもの |
|---|---|
| Avatar Root | 本体アバター |
| Attachment Point | 本体側の取り付け先ボーン (左腕なら `Chest`) |
| Sub Limb Root | **サブアバター内の部位サブツリーのルート** (例: Anubis の `Shoulder.L`) |
| Advanced > **Wrapper Root** | **サブアバター全体の prefab ルート** (例: Anubis prefab ルート) |

接続後、サブアバターの他の部位 (体・頭・脚など) は Wrapper Root 配下にそのまま残るので、不要な部位を後から手動で削除/非表示にできます。

---

## 自動マッピングの対応範囲

ベースアバターが Humanoid 設定済みの場合、以下の命名規則を自動で吸収します (`BoneNameNormalizer`)。

| 命名規則 | 例 |
|---|---|
| Unity Humanoid 標準 | `LeftUpperArm` / `RightHand` |
| `_ . - 半角空白` 区切り | `Upper_arm.L` `Lower Arm.L` `Hand-R` |
| Blender 重複サフィックス | `Hand.L.001` `Arm.L.002` |
| Mixamo | `mixamorig:LeftHandIndex1` `LeftUpLeg` `Pinky` |
| VRoid Studio | `J_Bip_L_UpperArm` `J_Bip_C_Neck` `J_Sec_*` |
| 3DS Max biped | `Bip01_L_Hand` `Bip001_L_Forearm` |
| 同義語 | `UpperArm↔Arm` `ForeArm↔LowerArm` `Thigh↔UpperLeg` `Calf↔Shin` 等 |
| 指関節短縮 | `Proximal/Intermediate/Distal/Metacarpal` |

対応外の命名 (日本語ボーン `腕.L` など) は赤 (Unmapped) と表示されるので、右側の ObjectField で手動オーバーライドしてください。

---

## Modular Avatar との連携

Limb Rigger は **Constraint 生成までを責務** としており、以下は範囲外です。これらは [Modular Avatar](https://modular-avatar.nadena.dev/) で実現してください。

| やりたいこと | 使う MA コンポーネント |
|---|---|
| 本体側の元部位 (元の腕) を非表示にする | `MA Mesh Settings` または `MA Object Toggle` |
| サブパーツの ON/OFF を Expression Menu に追加 | `MA Object Toggle` + `MA Menu Item` |
| Animator やマテリアル統合 | `MA Merge Animator` / `MA Material Setter` 等 |

「適用」ボタン押下後にも、ウィンドウ内に MA への案内 HelpBox が表示されます。

---

## 既知の制限

- ベースアバターは Humanoid 必須 (Generic ベースは Tier 2 名前マッチのみとなり非推奨)
- Edit モードでの動作プレビューは VRChat SDK の Constraint Preview 設定に依存 (`Project Settings → VRChat SDK → Constraints`)
- サブアーム/義手の表示 ON/OFF 切替の自動生成 (Modular Avatar 統合) は未実装。次バージョン以降の予定

---

## 動作確認済みの組み合わせ

- ベース: BeroarN `bn0010_Marycia` (Marycia / Humanoid)
- サブアーム: 如月開発 `Titan.V2 / Titan2arm` (両腕一体サブアーム)
- 上記の組合せで 28 ボーンが自動マッピングされ、VRChat 内で Hand Gesture 連動の指追従まで確認済み

---

## ライセンス

MIT License

## サポート

X (Twitter): [@iori__9973](https://x.com/iori__9973)
