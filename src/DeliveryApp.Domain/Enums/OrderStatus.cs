namespace DeliveryApp.Domain.Enums;

public enum OrderStatus
{
    Pending = 0, //aguardando pagamento
    Confirmed = 1, //pagamento confirmado
    Preparing = 2, //em preparação
    Shipped = 3, //enviado para entrega
    Delivered = 4, //entregue
    Cancelled = 5 //cancelado,
}
