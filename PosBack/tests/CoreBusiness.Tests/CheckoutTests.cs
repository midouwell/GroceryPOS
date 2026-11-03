using Payments.Api.Application;
using Payments.Api.Domain;
using Sales.Api.Application;

namespace CoreBusiness.Tests;

public sealed class CheckoutTests
{
    [Fact]
    public void CalculatesSubtotalTaxAndGrandTotal()
    {
        var totals = CheckoutCalculator.Calculate([
            (UnitPriceCents: 299, TaxRate: 0.10m, Quantity: 2),
            (UnitPriceCents: 125, TaxRate: 0.20m, Quantity: 1)
        ]);

        Assert.Equal(723, totals.SubtotalCents);
        Assert.Equal(85, totals.TaxCents);
        Assert.Equal(808, totals.GrandTotalCents);
    }

    [Fact]
    public async Task CashPaymentCalculatesChange()
    {
        var gateway = new CashPaymentGateway();
        var payment = new Payment { AmountCents = 808, TenderedCents = 1000 };

        var result = await gateway.ProcessAsync(payment, CancellationToken.None);

        Assert.Equal(PaymentStatuses.Completed, result.Status);
        Assert.Equal(192, result.ChangeCents);
    }

    [Fact]
    public async Task CashPaymentFailsWhenTenderedAmountIsInsufficient()
    {
        var payment = new Payment { AmountCents = 808, TenderedCents = 800 };

        var result = await new CashPaymentGateway().ProcessAsync(payment, CancellationToken.None);

        Assert.Equal(PaymentStatuses.Failed, result.Status);
        Assert.Equal(0, result.ChangeCents);
    }

    [Fact]
    public async Task CardPaymentReturnsExplicitMockStatus()
    {
        var payment = new Payment { AmountCents = 808, TenderedCents = 808 };

        var result = await new MockCardPaymentGateway().ProcessAsync(payment, CancellationToken.None);

        Assert.Equal(PaymentStatuses.Mock, result.Status);
        Assert.Equal("MOCK_CARD_TERMINAL_NOT_AUTHORIZED", result.ProviderReference);
    }
}
