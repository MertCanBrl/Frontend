namespace Backend.Models.DTOs;

public class SaveGradesRequest
{
    public decimal? Midterm { get; set; }   // Vize
    public decimal? Final { get; set; }      // Final
    public decimal? MakeUp { get; set; }     // Bütünleme
}