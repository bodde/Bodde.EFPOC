namespace Bodde.EFPOC.Entities
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProjectProduct> ProductProjects { get; set; }
    }
}