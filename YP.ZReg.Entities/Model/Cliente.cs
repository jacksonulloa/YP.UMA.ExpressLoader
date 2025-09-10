namespace YP.ZReg.Entities.Model
{
    public class Cliente
    {
        public long id { get; set; }
        public string nombre { get; set; } = string.Empty;
        public List<Deuda> deudas { get; set; } = [];
    }
}
