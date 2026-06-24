using ECommerce.Payment.Application.DTOs;
using ECommerce.Payment.Application.Features.Queries;
using ECommerce.Payment.Application.Interfaces;
using MediatR;

namespace ECommerce.Payment.Application.Features.Handlers;

public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto?>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentByOrderIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByOrderIdAsync(request.OrderId, cancellationToken);
        if (payment == null) return null;

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Status = payment.Status,
            FailureReason = payment.FailureReason,
            CardLastFour = payment.CardLastFour,
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt
        };
    }
}

public class GetPaymentByIdQueryHandler : IRequestHandler<GetPaymentByIdQuery, PaymentDto?>
{
    private readonly IPaymentRepository _paymentRepository;

    public GetPaymentByIdQueryHandler(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<PaymentDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(request.PaymentId, cancellationToken);
        if (payment == null) return null;

        return new PaymentDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Status = payment.Status,
            FailureReason = payment.FailureReason,
            CardLastFour = payment.CardLastFour,
            CreatedAt = payment.CreatedAt,
            ProcessedAt = payment.ProcessedAt
        };
    }
}
