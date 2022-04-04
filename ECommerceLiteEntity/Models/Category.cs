using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    [Table("Categories")]
    public class Category:Base<int>
    {
        [Required]
        [StringLength(100,MinimumLength=2,ErrorMessage="Kategori adı 2 ile 100 karakter arasında olmalıdır.")]
        [Display(Name ="Kategori Adı")]
        public string CategoryName { get; set; }
        [Required]
        [StringLength(500, MinimumLength = 2, ErrorMessage = "Kategori adı 2 ile 500 karakter arasında olmalıdır.")]
        [Display(Name = "Kategori Açıklaması")]
        public string CategoryDescription { get; set; }

        public int? BaseCategoryId { get; set; }// int normalde asla null değer alamaz! int in yanına ? yazarsak Nullable bir int 
        //oluşturmuş oluruz
        //public Nullable<int> BaseCategoryId {get;set;}
        
        [ForeignKey("BaseCategoryId")]
        public virtual Category BaseCategory { get; set; }

        public virtual List<Category>SubCategoryList { get; set; }

        //Her ürünün bir kategorisi olur cumlesınden yola cıkarak Productta tanımlanan ilişkiyi burada karşılayalım
        /*1'e sonsuz ilişki nedeniyle yeni bir kategorinin birden cok urunu olabilir mantıgını karsılamak amacıyla  burda
        virtual InversePropertyAttribute list tipindedir.*/

        public virtual List<Product>ProductList { get; set; }
        
    }
}
