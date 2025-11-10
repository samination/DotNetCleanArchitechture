using Domain.Entitites.Products;
using Domain.Helpers;
using Domain.Helpers.Exceptions;

namespace Domain.Entitites.Orders
{
    public class Order : Base
    {
        private Guid _productId;
        private PaymentStatus _paymentStatus = PaymentStatus.Pending;
        private DateTime _createdAt = DateTime.UtcNow;
        private DateTime? _paidAt;

        protected Order()
        {
        }

        public Order(Guid productId)
        {
            ProductId = productId;
            _paymentStatus = PaymentStatus.Pending;
            _createdAt = DateTime.UtcNow;
        }

        public Guid ProductId
        {
            get => _productId;
            set => _productId = Guard.AgainstEmpty(value, nameof(ProductId));
        }

        public Product Product { get; private set; } = null!;

        public PaymentStatus PaymentStatus => _paymentStatus;

        public DateTime CreatedAt => _createdAt;

        public DateTime? PaidAt => _paidAt;

        public void MarkPending()
        {
            _paymentStatus = PaymentStatus.Pending;
            _createdAt = DateTime.UtcNow;
            _paidAt = null;
            MarkUpdated();
        }

        public void MarkPaid(DateTime paidAt)
        {
            if (_paymentStatus == PaymentStatus.Paid)
            {
                throw new CustomException("The order has already been paid.");
            }

            _paymentStatus = PaymentStatus.Paid;
            _paidAt = paidAt;
            MarkUpdated();
        }
    }
}

