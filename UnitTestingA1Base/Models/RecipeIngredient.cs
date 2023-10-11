namespace UnitTestingA1Base.Models
{
    public class RecipeIngredient
    {
        public int IngredientId {  get; set; } 
        public int RecipeId { get; set; }
        public string Name { get; set; }
        public double Amount {  get; set; } 
        public MeasurementUnit MeasurementUnit { get; set; }   
    }

    public enum MeasurementUnit
    {
        Grams,
        Milliletres
    }
}
