namespace Calculator.Api.Entities;

public class UserCalcLog
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public string Operation { get; set; } = null!;
    public double OperandA { get; set; }
    public double OperandB { get; set; }
    public double Result { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
