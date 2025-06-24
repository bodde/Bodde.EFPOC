using System.Net.Http.Headers;

namespace Bodde.EFPOC.Entities
{
    public class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProjectProduct> ProjectProducts { get; set; }
    }
}
