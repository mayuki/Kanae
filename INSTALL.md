Installation
============

クイックスタート
---------------
1. git cloneやDownload Zipリンクでソースコード一式を取得します
2. Unity.sample.config, ConnectionString.sample.config, AppSettings.sample.config をそれぞれ Unity.config, ConnectionString.config, AppSettings.config にコピーします
3. Unity.config を開いて利用するストレージとデータベースのalias要素のコメントアウトを外します(その場合、別なところはコメントアウトしてください)
4. Windows Azure や Amazon S3 を利用する場合には AppSettings.config の各サービスの項目を設定します
5. SQL Server/SQL Database/SQL Server Compact Edition を利用する場合には ConnectionString.config の接続文字列を設定します
6. AppSettings.config を開いて認証について設定します(下記、認証設定についてを参照してください)
7. サインインして利用できるユーザーの設定します(下記、サインインについてを参照してください)
8. SQL Server/Windows Azure SQL Database/SQL Server Compact Editionを使う場合には下のほうにあるデータベースの作成を実行します

サインインについて
-----------------
アプリケーションではサインインして利用できるユーザーを限定することができます。
サンプルの設定ではサインインしても誰も利用できないようになっています。

設定は AppSettings.config の2つの項目を変更することで行えます。

#### Kanae:User:ClaimType: クレームの種類
認証した際にわたってくる情報にはサービス固有のIDや名前、場合によってはメールアドレスなどがあります。
そこでどの情報を利用してユーザーを制限するかを指定するのがKanae:User:ClaimTypeです。

* http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier: サービス固有のID
    * Twitter: TwitterのID(数値)
    * Google: ユーザーの識別するURI
    * Microsoft: ユーザーを識別するID
    * Facebook: ユーザーを識別するID
* http://schemas.xmlsoap.org/ws/2005/05/identity/claims/email: メールアドレス
    * Google: ユーザーのメールアドレス
    * WS-Federation: ユーザーのメールアドレス
* http://schemas.microsoft.com/ws/2008/06/identity/claims/primarysid: プライマリSID
    * 統合Windows認証: ユーザーのプライマリSID (ユーザーの固有のID)

#### Kanae:User:ClaimValueMatch: 値にマッチさせる文字列(正規表現)
指定したクレームの種類に入っている値にマッチさせる正規表現です。
例えば、以下のような指定ができます。

* Twitterでサインインする設定でクレームの種類にnameidentifierを指定している場合、^3613281$ とするとID(URLの英数字ではないほう)が 3613281 のユーザーのみが利用できます
* Googleでサインインする設定でクレームの種類にemailを指定している場合、マッチ文字列を @example\.com$ すると、@example.com のメールアドレスのユーザーのみが利用できます

.* にしたり ^$ を忘れたりすると意図しないユーザーが利用できてしまうため十分ご注意ください。

ただし統合Windows認証を利用している場合には .* を指定して、ACLその他でアクセス制限をかけるのがお手軽です。

認証設定について
---------------
#### MicrosoftアカウントTwitter,Google,Facebookでのサインイン
外部認証プロバイダを利用する場合には AppSettings.config の Kanae:AuthenticationProviders を設定します。
Kanae:AuthenticationProviders の値には利用したいプロバイダをカンマ区切りで指定します(Google,Microsoft,Twitter,Facebookを指定可能です)。

Googleアカウント以外での認証にはアクセストークンが必要となりますので各開発者向けサイトで取得してください。

#### 統合Windows認証を使う
統合Windows認証を使うとActive DirectoryやワークグループのWindowsの認証状態を使っての自動的にサインインとなります。
ただし統合Windows認証と外部認証プロバイダはどちらかの利用のみとなります。

統合Windows認証を利用するには下記の手順を実行してください。

* Web.config の system.web/authentication の mode属性 を None から Windows に変更
* AppSettings.config の Kanae:UseIntegratedWindowsAuthentication を true に変更
* (IIS Express+Visual Studioの場合) Kanae.Web のプロパティの Development Server から Windows Authentication を Enabled に、Anonymous Authentication を Disable に変更


SQL Server/Windows Azure SQL Database/SQL Server Compact Editionを使う
----------------------------------------------------------------------
SQL Server/Windows Azure SQL Database/SQL Server Compact Editionを利用する際にはあらかじめデータベースを作成する必要があります。
データベースは下記の手順で作成できます。

1. Kanae.Web 直下で ConnectionString.sample.config を  ConnectionString.config にコピーして修正
2. Package Manager Console でプロジェクト Kanae.Migration を選択
3. スタートアッププロジェクトを Kanae.Web に設定
4. Package Manager Console で Update-Database を実行


gitを使ってプライベートリポジトリにカスタマイズを保存する
-----------------------------------------------------
設定を保存したり、テンプレートをカスタムしたい!でも更新もできるようにしたい!
というときにはgitをうまく使うことでプライベートリポジトリなどを利用できます。

#### リポジトリをclone
    $ git clone https://github.com/mayuki/Kanae.git MyKanae

#### originをプライベートリポジトリに変更
次にリポジトリのoriginをupstreamという名前にして、originを自分のプライベートリポジトリにします。
こうすることでオリジナルのKanaeはupstreamにやってきて、originはプライベートリポジトリなのでpushしても向かうのはoriginなので手元で改修できます。ここではプライベートリポジトリはbitbucketにあるとしています。

    $ cd MyKanae
    $ git remote rename origin upstream
    
    $ git remote -v
    upstream        https://github.com/mayuki/Kanae.git (fetch)
    upstream        https://github.com/mayuki/Kanae.git (push)
    
    $ git remote add origin https://mayuki@bitbucket.org/mayuki/mykanae.git
    
    $ git remote -v
    origin  https://mayuki@bitbucket.org/mayuki/mykanae.git (fetch)
    origin  https://mayuki@bitbucket.org/mayuki/mykanae.git (push)
    upstream        https://github.com/mayuki/Kanae.git (fetch)
    upstream        https://github.com/mayuki/Kanae.git (push)
    
    $ git branch -a
    * master
      remotes/upstream/HEAD -> upstream/master

#### 最後にoriginに現在のmasterをpush
    $ git push -u origin master

-u オプションは超重要なので注意。masterが追いかける先をoriginにセットするオプション(そのままだとupstreamになってしまう)。

git config --list や git branch -vv で master が origin/master に向いてることを確認しましょう。

### 更新方法
オリジナルのKanaeが更新されたらupstream(オリジナル)を引っ張ってきてマージすれば最新の状態にできます。

#### まずupstreamの最新の状態を取得します。
    $ git fetch upstream

#### 次に手元の作業ブランチ(何もしてないときはmaster)にマージします
    $ git merge upstream/master
