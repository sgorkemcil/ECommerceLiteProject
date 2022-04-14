namespace ECommerceLiteDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ProductPictureTableUpdated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ProductPictures", "ProductPicture2", c => c.String(maxLength: 400));
            AddColumn("dbo.ProductPictures", "ProductPicture3", c => c.String(maxLength: 400));
            AddColumn("dbo.ProductPictures", "ProductPicture4", c => c.String(maxLength: 400));
            AddColumn("dbo.ProductPictures", "ProductPicture5", c => c.String(maxLength: 400));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ProductPictures", "ProductPicture5");
            DropColumn("dbo.ProductPictures", "ProductPicture4");
            DropColumn("dbo.ProductPictures", "ProductPicture3");
            DropColumn("dbo.ProductPictures", "ProductPicture2");
        }
    }
}
