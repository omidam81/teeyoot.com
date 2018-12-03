namespace Teeyoot.Module.ViewModels
{
    public class OrderProductViewModel
    {
        public int ProductId { get; set; }
        public int Count { get; set; }
        public int SizeId { get; set; }
        public int ProductSizeId { get; set; }
        public double Price { get; set; }
        public int ColorId { get; set; }
        public int CurrencyId { get; set; }
    }   
}
