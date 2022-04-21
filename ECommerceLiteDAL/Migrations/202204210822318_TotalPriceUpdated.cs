namespace ECommerceLiteDAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TotalPriceUpdated : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.OrderDetails", "TotalPrice", c => c.Decimal(nullable: false, precision: 18, scale: 2));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.OrderDetails", "TotalPrice", c => c.Double(nullable: false));
        }
    }
}
