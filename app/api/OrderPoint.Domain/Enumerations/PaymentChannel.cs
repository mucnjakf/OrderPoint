namespace OrderPoint.Domain.Enumerations;

// How a payment came in: cash or a card on the bar's terminal at the register, or a guest's card online
public enum PaymentChannel
{
    Cash,
    CardAtRegister,
    CardOnline
}