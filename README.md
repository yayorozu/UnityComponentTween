# UnityComponentTween

Animator がエンジニア的には使いづらく、UI の動きを DoTween 等を利用してコードで作成するのは調整が面倒という理由から、  
Inspector 上でパラメータをセットするとそれに合わせた動作を実現するツールを作成しました。  
※ 有料のツールが存在したかもしれませんが、こちらは無料で利用できます。  
また、拡張性にも配慮して作られているため、必要に応じて Module を追加することも可能です。

<img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20201004/20201004022746.png" width="500" alt="UnityComponentTween">

---

## 使い方

1. **Component Tween Sequence の追加**  
   動かしたい GameObject に `Component Tween Sequence` コンポーネントを追加します。

   <img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20201004/20201004023116.png" width="500" alt="Component Tween Sequence の追加">

2. **シーケンスの設定**  
   - Sequence の横にある **＋** ボタンを押して、動作（シーケンス）を追加します。  
   - `Start` （開始タイミング）と `Length`（動作の時間）を設定します。
   - 動作の対象として、どの Module を使用するかを選択します。  
     例: Transform の位置を変更する場合は `TransformPosition` を選択します。

   <img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20201004/20201004023502.png" width="500" alt="シーケンスの追加">
   <img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20201004/20201004023618.png" width="300" alt="Module の選択">

3. **パラメータの設定**  
   - **Beginning（開始値）** と **End（終了値）** を設定します。  
   - 特定の値を固定したい場合は **Lock** にチェックを入れます。  
   - 現在の位置から相対的に動作させたい場合は **IsRelative** にチェックを入れます。  
   - `Beginning` から `End` への値の変動は Easing を利用しており、好みの Easing を選択できます。  
     （内部的には Animation Curve で計算されています）

   <img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20201004/20201004023815.png" width="300" alt="パラメータの設定">
   <img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20201004/20201004023919.png" width="300" alt="Easing 設定">

4. **対象オブジェクトの指定**  
   最後に、動作対象となる `TargetObjects` をセットします。  
   例として Cube を作成し、これを対象オブジェクトとして設定します。

   <img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20201004/20201004024042.png" width="300" alt="TargetObjects の設定">

5. **動作の確認**  
   上部の **Play** ボタンを押すか、Editor を再生することで、設定した動作が実行されます。

   <img src="https://cdn-ak.f.st-hatena.com/images/fotolife/h/hacchi_man/20201004/20201004032538.gif" width="300" alt="動作の確認">

---

## スクリプトでの制御

各シーケンスは `ComponentTween` クラスが管理しているので、登録した ID を指定して以下のように制御できます。

- 停止する場合:
```csharp
  ComponentTween.Stop("ID");
```
- 再生する場合:
```csharp
ComponentTween.Play("ID");
```

また、終了時に処理を呼び出すためのイベントも用意されています。
```cs
public event CompleteDelegate CompleteEvent;
```

※ Loop する設定の場合は、終了イベントが呼び出されないので注意してください。

# Module の追加
新しい動作を追加する場合は、ModuleAbstract を継承したクラスを作成すると、Inspector の一覧に自動的に表示されます。
以下は、RectTransform のモジュールを作成する例です。

- RectTransformModule の作成
RectTransform に対する基本的な処理を記述するモジュールのベースクラス:

```csharp
[Serializable]
public abstract class RectTransformModule : ModuleAbstract
{
    [NonSerialized]
    protected RectTransform[] Components;

    protected override int GetComponent(GameObject[] objs)
    {
        Components = GetComponentsToArray<RectTransform>(objs);
        return Components.Length;
    }
}
```
- RectTransformPosition の作成
RectTransform の位置を制御するためのモジュール:

```csharp
[Serializable]
public class RectTransformPosition : RectTransformModule
{
    public override Type ParamType => typeof(Vector2);

    protected override Vector4[] GetValue()
    {
        return Components.Select(c => TweenValue.Vector2ToVector4(c.anchoredPosition))
                         .ToArray();
    }

    protected override void SetValue(TweenValue[] values)
    {
        for (var i = 0; i < values.Length; i++)
            Components[i].anchoredPosition = values[i].GetVector2();
    }
}
```
このように、Module を追加することで、さまざまな UI や Transform のパラメータに対して柔軟にアニメーションを適用できます。

# ライセンス

本プロジェクトは [MIT License](LICENSE) の下でライセンスされています。  
詳細については、LICENSE ファイルをご覧ください。
