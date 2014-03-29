Kanae
=====

共有したURLからのみ画像を閲覧することのできる画像アップローダーです。
いわゆるGyazoの設置型サーバーアプリケーションです。

特徴と機能
----------
- ハイパーゆるふわ実装
    - アクセス制御と画像さえちゃんと消えればいいポリシー
- 選べる認証プロバイダー
- 選べるストレージバックエンド
- 選べるデータベース
- 選べるクラウドまたはオンプレミス
- 選べるサーバープラットフォーム
- アップロードした画像の一覧表示
- アップロードした画像の簡易編集
    - 描き込み
    - 切り抜き
- 利用できるユーザーや認証時のドメインの設定
    - Windows Azure Active Directory/Office 365/Google Appsアカウントで特定のドメインのみなど
- 閲覧に認証の有無の設定
- 閲覧可能期間の設定
    - 例えば24時間のみ有効な画像等
- ストレージがWindows Azure Storage BlobやAmazon S3の場合、ストレージからデータの直接配信
- ブラウザからの画像アップロード
    - フォーム
    - ドラッグアンドドロップ
    - クリップボード (Internet Explorer 11+, Google Chrome)


やるお&やりたいお
-----------------
- 画像回り
    - 範囲選択の改善(スクロールとか)
    - Undo
    - 矩形などの図
    - 新規画像作成
    - 画像貼り付け
- PSDサポート
- 吹き出しコメント
- ほかのユーザーによる書き込み
- 管理者機能

サポートブラウザー
------------------
- Internet Explorer 11+
- Google Chrome 34+
- その他モダンブラウザ

選べる認証プロバイダー
----------------------
- Google
- Twitter
- Microsoft アカウント
- Facebook
- 統合Windows認証
- [予定] WS-Federation (Windows Azure Active Directory/Office 365) IdentityがPre-Releaseから上がったら対応予定

選べるストレージバックエンド
----------------------------
- Windows Azure Storage Blob
- Amazon S3
- ファイルシステム

選べるデータベース
------------------
- Windows Azure Storage Table
- Microsoft SQL Server (Windows Azure SQL Database)
- SQL Server Compact Edition
- ファイルシステム (ストレージがファイルシステムの時のみ)
- [対応するかも] SQLite (EF Migrationsに対応したらSQL CEの代わりに…)

選べるクラウドまたはオンプレミス
--------------------------------
- Windows Azure
- Amazon EC2
- オンプレミス

選べるサーバープラットフォーム
------------------------------
- Windows 8
- Windows 8 Pro
- Windows 8.1
- Windows 8.1 Pro
- Windows Web Server 2008
- Windows Server 2008 Standard Edition
- Windows Server 2008 Enterprise Edition
- Windows Server 2008 Datacenter Edition
- Windows Web Server 2008 R2
- Windows Server 2008 R2 Standard Edition
- Windows Server 2008 R2 Enterprise Edition
- Windows Server 2008 R2 Datacenter Edition
- Windows Server 2012 Standard Edition
- Windows Server 2012 Datacenter Edition
- Windows Server 2012 Essentials
- Windows Server 2012 R2 Standard Edition
- Windows Server 2012 R2 Datacenter Edition
- Windows Server 2012 R2 Essentials
- Microsoft .NET Framework 4.5
- Microsoft .NET Framework 4.5.1

構成組み合わせ例
----------------
### Windows Azure
- Windows Azure Storage Table + Windows Azure Storage Blob + Microsoft アカウント認証
- Windows Azure SQL Database + Windows Azure Storage Blob + Microsoft アカウント認証

### Amazon S3
- SQL Server + Amazon S3 + Google 認証
- SQL Server Compact Edition + Amazon S3 + 統合Windows認証

### お手軽構成
- SQL Server Compact Edition + ファイルシステム + Google 認証
- SQL Server Compact Edition + ファイルシステム + 統合Windows認証

### エンタープライズ
- SQL Server + ファイルシステム + 統合Windows認証

### なんでもあり
- Windows Azure Storage Table + Amazon S3 + 統合Windows認証

インストールメモ (SQL Server/Windows Azure SQL Database/SQL Server Compact Edition)
-----------------------------------------------------------------------------------
1. Kanae.Web 直下に ConnectionString.sample.config を  ConnectionString.config にコピーして修正
2. Package Manager Console でプロジェクト Kanae.Migration を選択
3. スタートアッププロジェクトを Kanae.Web に設定
4. Package Manager Console で Update-Database を実行

LICENSE
-------
MIT License