# はじめに
この講習を始める前に、使用するエディタUnity6(6000.0.40f1)のインストールを行い、リポジトリのクローンも合わせてお願いします。

この資料は、C#やUnityに関してある程度知識のある人が読むことを想定して作成しています。内容がわかりにくい場合はUnityができる人と一緒に取り組むことを推奨します。

おそらくエディタの初回起動時にエラーが出ますが、一度Ignoreを押してからエディタを閉じて開きなおしてください。(初回起動時はNugetのパッケージが解決されないためにエラーが出ます)

# Extenjectとは何か
Extenject(旧Zenject)は、Unityで依存性注入を実現するために用いられるライブラリです。
## 依存性注入(DI)とは
依存性注入(Dependency Injection, DI)は、デザインパターンの一種です。
以下はWikipediaでの説明です。
> あるオブジェクトや関数が、依存する他のオブジェクトや関数を受け取るデザインパターンである

> 制御の反転の一種で、オブジェクトの作成と利用について関心の分離を行い、疎結合なプログラムを実現することを目的としている

なんとなく分かるような分からないようなという感じです。以下で説明していきます。

## 何が嬉しいのか
DIは何が嬉しいのでしょうか。

### 疎結合なプログラムになる
まず、DIを使うと疎結合なプログラムを作ることができます。

疎結合なプログラムはシステムの構成要素の独立性を高め、他の構成要素との依存関係を弱くします。

これにより、プログラムが複雑になることを抑制し、簡潔なプログラムを書くことができるようになります。

### テストがしやすくなる
以下のようなコードがあったとしましょう。

```c#
public class Example
{
    private DataFetcher dataFetcher = DataFetcher.Singleton;

    public int GetData()
    {
        if (dataFetcher.Fetch() == 0)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}
```
例えば`DataFetcher.Fetch()`が外部からデータを取得するもので、その回数によって課金されるなどの場合、気軽にテストすることができなくなってしまいます。

そこで、テスト用にダミーの値を返すクラスを別途用意して、テストの際にはテスト用のクラスを使うことを考えてみます。そのようにすれば、気軽にテストができるようになります。

クラスを入れ替えるということを考えたとき、まず思いつくのはインターフェースを作ることです。`IDataFetchable`というインターフェースを作って、それを実装した`DataFetcher`と`TestDataFetcher`を用意してあげれば、それらを入れ替えることで簡単にテストできます。

しかし、どのようにして入れ替えるのかというのが難しいです。テストを行うたびに`IDataFetcher`が書かれている部分を書き換えるのは面倒すぎます。

ここで登場するのがDIです。DIを使うと、先ほどのコードは以下のように書くことができます。

```c#
public class Example
{
    [Inject] private IDataFetchable dataFetcher;

    public int GetData()
    {
        if (dataFetcher.Fetch() == 0)
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
}
```

どうでしょうか。`Example`クラスは`dataFetcher`に格納されているオブジェクトがテスト用のものであるかどうかを全く気にしなくてよいことがわかります。

このように、DIを使うと注入するクラスを変更するだけでテストを行えるようになります。

本番で使うコードからテスト用の記述を完全に排除できる点もポイント高いです。

### MonoBehaviorの無駄な継承を減らせる
UnityでExtenjectを使う利点として、`MonoBehavior`の無駄な継承を減らすことができるというものがあります。

よくUnityの解説などで、"Manager"という名前の空のオブジェクトを作成して、そこに`MonoBehavior`を継承した`GameManager`のようなクラスをアタッチしているのを見かけます。

しかし、`GameManager`が`MonoBehavior`を継承し、ゲームオブジェクトにアタッチされる必要はあるでしょうか。その必要はないことがほとんどだと思います。

Extenjectを使うと`MonoBehavior`を継承せず、ヒエラルキーに"Manager"という名前のオブジェクトを置くこともなくManager系のクラスを定義することができます。

後述しますが、`MonoBehavior`を継承せずとも`Start()`や`Update()`などに相当するメソッドを実装する方法をExtenjectが用意してくれているので、ライフサイクルに準じた処理を書くこともできます。

# DIしてみよう
実際にDIしてみましょう。

## 講座に使用するリポジトリのクローン
以下のリポジトリをクローンしてブランチ`lecture-checkpoints/1`に入ってください。

https://github.com/tuatmcc/ExtenjectLecture

RougueBitというローグライク(?)なゲーム(未完)を作っていきます。

再掲: おそらくエディタの初回起動時にエラーが出ますが、一度Ignoreを押してからエディタを閉じて開きなおしてください。(初回起動時はNugetのパッケージが解決されないためにエラーが出ます)

## 前提条件
以下のようなディレクトリ構成になっていると仮定して話を進めていきます。存在しないディレクトリが説明に現れた際はProjectタブから適宜作成してください。また、C#スクリプトの作成はProjectタブでスクリプトを置きたいディレクトリを開いてその上で右クリックし、[Create] > [Scripting] > [{作りたいスクリプトの種別}] を選択することでできます。Unity6への更新でUIが少し変わっているので注意してください。

```text:ディレクトリ構成
Assets/
├─ RougueBit/
│   ├── Materials/
│   ├── Prefabs/
│   ├── Resources/
│   ├── Scenes/
│   ├── Scripts/
```

## プロジェクト全体でInjectを行う
プロジェクト全体でInjectを行う場面として、以下のようなものが考えられます。
- ゲーム全体の状態管理・データ保持
- 外部との通信管理

以下では実際に簡単なゲームの制作を通してInjectする例を見ていきます。

### Injectされるインターフェースを書く
`Assets/RougueBit/Scripts/Core/Interface`に`IGameStateManager.cs`を作り、ゲーム全体の状態を管理するインターフェース`IGameStateManager`を以下のように定義します。ゲーム全体の管理に必要なプロパティやメソッドを定義しています。

```c#:IGameStateManager.cs
using System;

namespace RougueBit.Core.Interface
{
    public interface IGameStateManager
    {
        public event Action<GameState> OnGameStateChanged;
        public GameState GameState { get; }
        public CoreInputs CoreInputs { get; }
        public void NextScene();
    }
}
```

合わせて、`Assets/RougueBit/Scripts/Core`に`GameStateManager.cs`を作り、以下の内容を書き込みます。
```c#:GameStateManager.cs
using R3;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Zenject;

namespace RougueBit.Core
{
    public class GameStateManager: IGameStateManager, IInitializable, IDisposable
    {
        public event Action<GameState> OnGameStateChanged;

        public GameState GameState
        {
            get => _gameState;
            private set
            {
                if (_gameState == value)
                {
                    return;
                }
                _gameState = value;
                OnGameStateChanged?.Invoke(_gameState);
            }
        }
        private GameState _gameState;

        public CoreInputs CoreInputs { get; set; } = new();

        private readonly CompositeDisposable disposables = new();

        // Awakeに相当
        public GameStateManager()
        {
            GameState = GameState.Title;
            CoreInputs.Enable();
        }

        // Startに相当
        public void Initialize()
        {
            GameState = GameState.Title;
            Observable.FromEvent<InputAction.CallbackContext>(
                h => CoreInputs.Main.Reset.performed += h,
                h => CoreInputs.Main.Reset.performed -= h
            ).Subscribe(_ => Reset()).AddTo(disposables);
            Observable.FromEvent<GameState>(
                h => OnGameStateChanged += h,
                h => OnGameStateChanged -= h
            ).Subscribe(TransitScene).AddTo(disposables);
        }

        public void NextScene()
        {
            switch (GameState)
            {
                case GameState.Title:
                    GameState = GameState.Playing;
                    break;
                case GameState.Playing:
                    GameState = GameState.Result;
                    break;
                case GameState.Result:
                    Reset();
                    break;
            }
        }

        private void TransitScene(GameState gameState)
        {
            switch (gameState)
            {
                case GameState.Title:
                    SceneManager.LoadScene("Title");
                    break;
                case GameState.Playing:
                    SceneManager.LoadScene("Play");
                    break;
                case GameState.Result:
                    SceneManager.LoadScene("Result");
                    break;
            }
        }

        private void Reset()
        {
            GameState = GameState.Title;
        }

        // OnDestroyに相当
        public void Dispose()
        {
            disposables.Dispose();
            CoreInputs.Dispose();
        }
    }
}
```

### ProjectContextを作る
プロジェクト全体でInjectを行うには`ProjectContext`が必要です。ディレクトリ`Assets/RougueBit/Resources`を作成し、右クリックして [Create] > [Zenject] > [Project Context] で`ProjectContext`を作成します。

見ればわかりますがこれはPrefabです。Injectが設定されていると、シーンの再生と同時に自動でDontDestroyOnLoadに生成されます。

### Installerを作る
Injectを行うには、Installerを書く必要があります。

`Assets/RougueBit/Scripts/Core/DI`に`GameStateManagerInstaller.cs`を作ります。[Create] > [Zenject] > [Mono Installer] から作成できます。

```c#:GameStateManagerInstaller.cs
using Zenject;

namespace RougueBit.Core.DI
{
    public class GameStateManagerInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesTo<GameStateManager>().FromNew().AsSingle();
        }
    }
}
```

基本的には以下のように書けばよいと思います

```c#
// 新しくインスタンスを作成してInjectするとき
Container.BindInterfacesTo<{InjectしたいInterfaceを実装したクラス名}>().FromNew().AsSingle();
// 既に存在してしるオブジェクトをInjectするとき
Container.BindInterfacesTo<{InjectしたいInterfaceを実装したクラス名}>().FromInstance({オブジェクトの変数}).AsSingle();
```

という書き方になると思います。上のコードを普通の書き方で書くと以下のようになります。

```c#
Container
    .Bind<typeof(IGameStateManager), typeof(IInitializable), typeof(IDisposable)>()
    .To<GameStateManager>()
    .FromNew()
    .AsSingle();
```

本来はバインドするインターフェースをすべて記述しなければならないのですが、`BindInterfacesTo`を使うと指定したオブジェクトが実装しているインターフェースを一括でバインドできます。

`FromNew`は必要なときに新しくインスタンスを作ってInjectするもので、`FromInstance`は既に存在しているインスタンスをInjectするものです。

`AsSingle`は注入が行われるときにインスタンスを再利用しないもので、同じクラスに対してBindが2回以上呼ばれると例外となります。

他の記法については以下が参考になると思います。

https://light11.hatenadiary.com/entry/2019/02/22/005845

### Injectの設定をしよう
`Assets/RougueBit/Resources`に作成した`ProjectContext`を開き、以下の手順でInjectの設定をします。

- `ProjectContext`に`GameStateManagerInstaller`をアタッチする(Add componentを押して追加する)
- スクリプト`ProjectContext`の`Mono Installers`の`+`ボタンを押す
- 増えた部分に、**先ほどアタッチした**`GameStateManagerInstaller`をドラッグアンドドロップする

これでInjectが行われるようになりました。

### Injectされてみよう
これまでの記述でInjectができるようになりました。Injectされてみましょう。

`Assets/RougueBit/Scenes`に`Title`シーンが用意されているので開きましょう。ここにタイトルシーンを管理する`TitleManager`クラスを定義しましょう。`Assets/RougueBit/Title`に`TitleManager.cs`を作成し、以下の内容を書き込みます。

(`ITitleManager`は`Assets/RougueBit/Scripts/Title/Interface/ITitleManager.cs`に定義されています。)

```c#:TitleManager.cs
using R3;
using RougueBit.Core;
using System;
using UnityEngine.InputSystem;
using Zenject;

namespace RougueBit.Title
{
    public class TitleManager: ITitleManager, IInitializable, IDisposable
    {
        [Inject] private readonly IGameStateManager _gameStateManager;

        public TitleInputs TitleInputs { get; private set; } = new();

        private CompositeDisposable disposables = new();

        public TitleManager()
        {
            TitleInputs.Enable();
        }

        public void Initialize()
        {
            Observable.FromEvent<InputAction.CallbackContext>(
                h => TitleInputs.Main.Enter.performed += h,
                h => TitleInputs.Main.Enter.performed -= h
            ).Subscribe(_ => _gameStateManager.NextScene()).AddTo(disposables);
        }

        public void Dispose()
        {
            disposables.Dispose();
            TitleInputs.Disable();
        }
    }
}
```

Enterを押すと`Play`シーンに移動する処理が書かれています。

以下の部分に注目しましょう。`[SerializeField]`と同じように`[Inject]`という属性をつけてあげることで、このクラスのインスタンスが生成されたときに`IGameStateManager`が注入されます。これによって、`_gameStateManager`変数からIGameStateManagerにアクセスできます。

```c#
[Inject] private readonly IGameStateManager _gameStateManager; // Inject
```

また、Extenjectでは以下のように`MonoBehavior`におけるライフサイクルに準じたメソッドを実装するためのインターフェースが用意されています。

| MonoBehavior | 非MonoBehavior |
|:-:|:-:|
| `Awake` | コンストラクタ(C#標準) |
| `Start` | `Initialize`(`IInitializable`を実装) |
| `Update` | `Tick`(`ITickable`を実装) |
| `FixedUpdate` | `FixedTick`(`IFixedTickable`を実装) |
| `OnDestroy` | `Dispose`(C#標準`IDisposable`を実装) |

さて、Injectされるためのコードを書いたので`Title`シーンで動くようにしましょう。シーン上でInjectされるようにするには、各シーン上に`SceneContext`を置く必要があります。

`Title`シーンを開いたHierarchy上で右クリックして、[Zenject] > [Scene Context] と選択することで`SceneContext`を作ることができます。`Assets/RougueBit/Prefabs/Title`にドラッグアンドドロップしてプレハブにしておきましょう。

`SceneContext`のプレハブに入って`SceneContext`のInspectorを開きましょう。`ProjectContext`と同じように以下の手順で設定を行います。

- `SceneContext`に`TitleManagerInstaller`をアタッチする
- スクリプト`SceneContext`の`Mono Installers`の`+`ボタンを押す
- 増えた部分に、**先ほどアタッチした**`TitleManagerInstaller`をドラッグアンドドロップする

これでInjectされるようになりました。

`Title`シーンを再生してEnterキーを押してみましょう。Hierarchyを見ると`Play`シーンに移動していることがわかります。

### Injectの注意点
`[Inject]`属性を使用して問題なくInjectされるのは、基本的にシーンを再生した際に生成されるオブジェクトです。具体的には以下のような場合です。

- 再生時に、シーン上に置かれているオブジェクトにアタッチされた`MonoBehavior`を継承したクラスのオブジェクト
- Installerの設定によってシーン開始時に生成されたオブジェクト

これらとは反対に、`Instantiate`などで動的に生成されたプレハブにアタッチされた`MonoBehavior`を継承したクラスのオブジェクトでは正しくInjectされません。動的に生成されるプレハブ内のオブジェクトにInjectしたい場合は、そのプレハブに`ZenAutoInjector`というスクリプトをアタッチしましょう。

基本的なInjectのやり方はここまでの説明で終了です。以下では、Injectの具体的な活用方法について書いていきます。

## ブランチ移動
これまでの変更をStashするかDiscardして、ブランチ`lecture-checkpoints/2`に移動してください。

(PlaySceneOSが無いというエラーが出ますが、この後作成するので一旦無視してください)

## Injectを活用する

### Scriptable ObjectをInjectする
#### Scriptable Objectを作る
Unityには設定値などを簡単に扱うことができる`Scriptable Object`というものがあります。Extenjectを使えば`Scriptable Object`もInjectできます。

`Assets/RougueBit/Scripts/Play`に`Play`シーンに関する設定値を保持する`PlaySceneSO.cs`を作り、以下の内容を書き込みます。ステージ生成に関するパラメータです。

```c#:PlaySceneSO.cs
using UnityEngine;

namespace RougueBit.Play
{
    [CreateAssetMenu(fileName = "PlaySceneSettings", menuName = "Scriptable Objects/PlaySceneSettings")]
    public class PlaySceneSO : ScriptableObject
    {
        public int StageWidth => stageWidth;
        public int StageDepth => stageDepth;
        public int StageMinRoomSize => stageMinRoomSize;
        public int StageMaxRoomSize => stageMaxRoomSize;
        public int StageMaxRooms => stageMaxRooms;
        public GameObject WallPrefab => wallPrefab;
        public GameObject FloorPrefab => floorPrefab;

        [SerializeField] private int stageWidth = 50;
        [SerializeField] private int stageDepth = 50; // Z方向のサイズ
        [SerializeField] private int stageMinRoomSize = 5;
        [SerializeField] private int stageMaxRoomSize = 15;
        [SerializeField] private int stageMaxRooms = 10;
        [SerializeField] private GameObject wallPrefab;
        [SerializeField] private GameObject floorPrefab;
    }
}
```

保存してEditorに戻り、Projectタブで`Assets/RougueBit/Settings`を開いて右クリックし、[Create] > [Scriptable Objects] > [PlaySceneSettings] をクリックして`PlaySceneSettings`という名前を付けます。

これにより`PlaySceneSettings.asset`というファイルが生成され、Inspectorタブを見ると`SerializeField`に設定した各項目が見られるようになっていると思います。

ここで、`Wall Prefab`に`Assets/RougueBit/Prefabs/Play/Stage/Wall.prefab`を、`Floor Prefab`に`Assets/RougueBit/Prefabs/Play/Floor.prefab`をセットしておきます。

#### Installerを書く
続いてInjectする処理を書いていきましょう。`Assets/RougueBit/Scripts/Play/DI`に`PlaySceneSettingsInstaller.cs`を作り、以下の内容を書き込みます。

```c#:PlaySceneSettingsInstaller.cs
using UnityEngine;
using Zenject;

namespace RougueBit.Play.DI
{
    [CreateAssetMenu(fileName = "PlaySceneSettingsInstaller", menuName = "Installers/PlaySceneSettingsInstaller")]
    public class PlaySceneSettingsInstaller : ScriptableObjectInstaller<PlaySceneSettingsInstaller>
    {
        [SerializeField] private PlaySceneSO playSceneSO;

        public override void InstallBindings()
        {
            Container.BindInstance(playSceneSO).AsSingle();
        }
    }
}
```

先ほどのMonoInstallerと書き方はほとんど同じであることがわかります。

#### Injectの設定をする
今度は`Play`シーンに配置されている`SceneContext`のプレハブに対して、以下の手順で設定を行います。`MonoInstaller`とは手順が違い、少し複雑なため注意してください。

- Projectタブで`Assets/RougueBit/Scripts/Play/DI`を開き、右クリックして [Create] > [Installers] > [PlaySceneSettingsInstaller] を選択(名前はそのままでOK)
- 生成された`PlaySceneSettingsInstaller.asset`のInspectorを開き、`Play Scene SO`に`Assets/RougueBit/Settings/PlaySceneSettings.asset`をドラッグアンドドロップ
- `Play`シーン上に置かれている`SceneContext`のプレハブを開き、スクリプト`SceneContext`の`Scriptable Object Installer`の`+`ボタンをクリック
- 増えた部分に、`Assets/RougueBit/Scripts/Play/DI/PlaySceneSettingsInstaller.asset`をドラッグアンドドロップ(普通に選択してもよい)

#### Injectされるスクリプトを書く
`Play`シーンを管理するスクリプトを作ります。`Assets/RougueBit/Scripts/Play`に`PlaySceneManager.cs`を作り、以下の内容を書きこんでください。

```c#:PlaySceneManager.cs
using RougueBit.Play.Interface;
using System;
using UnityEngine;
using Zenject;

namespace RougueBit.Play
{
    public class PlayManager : IPlayManager, IInitializable, IDisposable
    {
        public PlayInputs PlayInputs { get; } = new();

        private readonly StageGenerator stageGenerator;

        [Inject]
        public PlayManager(PlaySceneSO playSceneSO)
        {
            stageGenerator = new(playSceneSO);
            PlayInputs.Enable();
        }

        public void Initialize()
        {
            stageGenerator.Generate();
        }

        public void Dispose()
        {
            PlayInputs.Dispose();
        }
    }
}
```

以下の部分に注目してください。このように、コンストラクタの引数にInjectすることも可能です。

```c#
[Inject]
public PlayManager(PlaySceneSO playSceneSO)
{
    stageGenerator = new(playSceneSO);
    PlayInputs.Enable();
}
```

ただし、`MonoBehavior`を継承したクラスの場合は通常のコンストラクタが使えないので、以下のようにメソッドに対するInjectで対処できます。`Construct`というメソッド名は何でもよく、Injectが行われたタイミングで実行されます

```c#
[Inject]
public void Construct(PlaySceneSO playSceneSO)
{
    stageGenerator = new(playSceneSO);
    PlayInputs.Enable();
}
```

#### 復習
`Play`シーン上で先ほど作成した`PlayManager.cs`が正しくInjectされるように設定してみてください。

#### 確認
`Play`シーンを再生してSceneタブで確認してみてください。再生するたびに部屋と通路がランダムに生成されていれば問題ありません。

### ブランチ移動
これまでの変更をStashするかDiscardして、ブランチ`lecture-checkpoints/3`に移動してください。

### テスト用のコードに切り替えてみる
PlayManagerをテスト用のコードに切り替えてみましょう。
`Assets/RougueBit/Scripts/Play/Tests`に`TestPlayManager.cs`を作り、以下の内容を書き込みます。

```c#:TestPlayManager.cs
using R3;
using RougueBit.Play.Interface;
using System;
using UnityEngine;
using Zenject;

namespace RougueBit.Play.Tests
{
    public class TestPlayManager : IPlayManager, IInitializable, IDisposable
    {
        public event Action<PlayState> OnPlayStateChanged;

        public PlayState PlayState
        {
            get => playState;
            private set
            {
                playState = value;
                OnPlayStateChanged?.Invoke(playState);
            }
        }


        public PlayInputs PlayInputs { get; } = new();

        private PlayState playState;
        private IStageGeneratable stageGenerator;
        private PlaySceneSO playSceneSO;
        private CompositeDisposable disposables = new();

        [Inject]
        public TestPlayManager(PlaySceneSO playSceneSO)
        {
            this.playSceneSO = playSceneSO;
            PlayInputs.Enable();
            stageGenerator = new TestStageGenerator(playSceneSO);
        }

        public void Initialize()
        {
            Observable.FromEvent<PlayState>(
                h => OnPlayStateChanged += h,
                h => OnPlayStateChanged -= h
            ).Subscribe(NextState).AddTo(disposables);
            PlayState = PlayState.GenerateStage;
        }

        private void NextState(PlayState nextState)
        {
            switch (nextState)
            {
                case PlayState.GenerateStage:
                    stageGenerator.Generate();
                    playSceneSO.PlayerStartPosition = stageGenerator.GetRandomFloor();
                    PlayState = PlayState.SetPlayer;
                    break;
                case PlayState.SetPlayer:
                    break;
                case PlayState.Playing:
                    break;
            }
        }

        public void Dispose()
        {
            disposables.Dispose();
            PlayInputs.Disable();
        }
    }
}
```

テスト用のステージ生成スクリプトを使って壁のないステージ生成をするようにしています。

このテスト用のManagerを使うように変更してみましょう。

`Assets/RougueBit/Scripts/Play/DI/PlaySceneInstaller.cs`の内容を以下に変更します。

```c#:PlaySceneInstaller.cs
using RougueBit.Play.Tests;
using UnityEngine;
using Zenject;

namespace RougueBit.Play.DI
{
    public class PlaySceneInstaller : MonoInstaller
    {
        [SerializeField] private bool isTest;

        public override void InstallBindings()
        {
            if (isTest)
            {
                Container.BindInterfacesTo<TestPlayManager>().AsSingle();
            }
            else
            {
                Container.BindInterfacesTo<PlayManager>().AsSingle();
            }
        }
    }
}
```

このように変更したことで`Play`シーンのSceneContextにアタッチされている`PlaySceneInstaller`に`IsTest`というチェックボックスが現れました。`isTest`が`true`ならテスト用の`TestPlayManager`が、`false`なら本番用の`PlayManager`がInjectされます。

実際に`IsTest`のチェックボックスをOn・Offの状態で`Play`シーンを再生してみてください。生成されるステージが変わっていることがわかると思います。

とても簡単になりましたが、実践は以上となります。

前回の学祭のUnity側リポジトリなどをのぞいてみると、さらに具体的な用法がわかるかとおもいます。(DIを活用しきれていない部分もありますが…)

https://github.com/tuatmcc/SchoolFestival2024_Unity

## その他Tipsなど
ここまでDIについて簡単に説明してきました。DIは非常に深いため著者が理解している部分はほんの一部に過ぎないことはご承知おきください。以下に今回の講習に関してTipsを書いておきます。

### Injectなどのタイミング
開発中は`NullReferenceException`が多く発生しますが、その一因としてInjectが行われる前に参照してしまうというものがあります。以下の記事などを参考に、Injectの設定が正しく、参照のタイミングに問題がないかも確認しましょう。

https://blog.virtualcast.jp/blog/2020/04/zenject_initialize_order_trap/

### Extenjectについて
今回使用した`Extenject`は、諸事情により更新が止まってしまった`Zenject`のForkです。しかし、`Extenject`はリポジトリの更新はされているものの、リリースが滞っており今後の動向には注意が必要かもしれません。代替のDIライブラリとして`VContainer`というものもあるため、Unityのバージョンアップにより`Extenject`が動かなくなったといった問題が発生した場合はこちらに切り替えることも検討しています。
