namespace UnitTestingA1Base.Models
{
    public class RecipeInput
    {
        public string Name { get; set; }
        public Recipe Recipe { get; set; }
        public List<RecipeIngredient> Ingredients { get; set; }
    }
}
