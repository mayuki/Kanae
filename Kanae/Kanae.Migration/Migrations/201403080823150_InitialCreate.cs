namespace Kanae.Migration.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MediaInfo",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        MediaId = c.Guid(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        ContentType = c.String(),
                    })
                .Index(t => t.UserId)
                .PrimaryKey(t => t.MediaId);
            
            CreateTable(
                "dbo.UserInfo",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        AuthHash = c.String(maxLength: 128),
                    })
                .Index(t => t.AuthHash, unique: true)
                .PrimaryKey(t => t.UserId);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserInfo");
            DropTable("dbo.MediaInfo");
        }
    }
}
