using ECommerceLiteEntity.IdentityModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceLiteEntity.Models
{
    [Table("Orders")]
    public class Order:Base<int>
    {
        [Required]
        [Display(Name ="Sipariş Numarası")]
        [StringLength(maximumLength:11,MinimumLength =11,ErrorMessage ="Sipariş numarası 11 haneli olmalıdır.")]
        public string OrderNumber { get; set; }

        //Kim bu siparişi verdi?
        public string CustomerTCNumber { get; set; }
        [ForeignKey("CustomerTCNumber")]
        public virtual Customer Customer { get; set; }
        

        //Bu siparişin içeriğinde neler var ?Ne almış ?

        public virtual List<OrderDetail> OrderDetails { get; set; }

    }
}
