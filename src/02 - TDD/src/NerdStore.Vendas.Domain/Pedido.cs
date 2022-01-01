using NerdStore.Core.DomainObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Results;

namespace NerdStore.Vendas.Domain
{
    public class Pedido: Entity
    {
        public static int MAX_UNIDADES_ITEM => 15;

        public static int MIN_UNIDADES_ITEM => 1;

        public Guid ClienteId { get; private set; }

        public decimal ValorTotal { get; private set; }

        public decimal Desconto { get; private set; }

        public PedidoStatus PedidoStatus { get; private set; }

        public bool VoucherUtilizado { get; private set; }

        public Voucher Voucher { get; private set; }

        public IReadOnlyCollection<PedidoItem> PedidoItems => _pedidoItems;

        public ValidationResult AplicarVoucher(Voucher voucher)
        {
            var result = voucher.ValidarSeAplicavel();

            if (!result.IsValid) return result;

            Voucher = voucher;
            VoucherUtilizado = true;

            CalcularValorTotalDesconto();

            return result;
        }

        public void CalcularValorTotalDesconto()
        {
            if (!VoucherUtilizado) return;

            decimal desconto = 0;
            var valor = ValorTotal;

            switch (Voucher.TipoDescontoVoucher)
            {
                case TipoDescontoVoucher.Valor:
                    {
                        if (Voucher.ValorDesconto.HasValue)
                            desconto = Voucher.ValorDesconto.Value;
                            valor -= desconto;
                        break;
                    }
                case TipoDescontoVoucher.Porcentagem:
                    {
                        if (Voucher.PercentualDesconto.HasValue)
                            desconto = (ValorTotal * Voucher.PercentualDesconto.Value) / 100;
                            valor -= desconto;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ValorTotal = valor < 0 ? 0 : valor;
            Desconto = desconto;
        }

        private List<PedidoItem> _pedidoItems;

        protected Pedido()
        {
            _pedidoItems = new List<PedidoItem>();
        }

        private void CalcularValorPedido()
        {
            ValorTotal = PedidoItems.Sum(i => i.CalcularValor());
            CalcularValorTotalDesconto();
        }

        public void AdicionarItem(PedidoItem pedidoItem)
        {
            ValidarQuantidadeItemPermitido(pedidoItem);

            if (PedidoItemExistente(pedidoItem))
            {
                var itemExistente = _pedidoItems.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId);

                itemExistente.AdicionarUnidades(pedidoItem.Quantidade);

                pedidoItem = itemExistente;

                _pedidoItems.Remove(itemExistente);
            }

            _pedidoItems.Add(pedidoItem);

            ValorTotal = PedidoItems.Sum(i => i.Quantidade * i.ValorUnitario);

            CalcularValorPedido();
        }

        public void AtualizarItem(PedidoItem pedidoItem)
        {
            ValidarPedidoItemInexistente(pedidoItem);

            ValidarQuantidadeItemPermitido(pedidoItem);

            var itemExistente = PedidoItems.FirstOrDefault(p => p.ProdutoId == pedidoItem.ProdutoId);

            _pedidoItems.Remove(itemExistente);
            _pedidoItems.Add(pedidoItem);

            CalcularValorPedido();
        }

        public void RemoverItem(PedidoItem pedidoItem)
        {
            ValidarPedidoItemInexistente(pedidoItem);

            _pedidoItems.Remove(pedidoItem);

            CalcularValorPedido();
        }

        public void TornarRaschunho()
        {
            PedidoStatus = PedidoStatus.Rascunho;
        }

        public bool PedidoItemExistente(PedidoItem item) => _pedidoItems.Any(p => p.ProdutoId == item.ProdutoId);

        public void ValidarPedidoItemInexistente(PedidoItem item)
        {
            if (!PedidoItemExistente(item))
                throw new DomainException($"O item não pertence ao pedido");
        }

        public void ValidarQuantidadeItemPermitido(PedidoItem item)
        {
            if (_pedidoItems.Sum(a => a.Quantidade) + item.Quantidade > MAX_UNIDADES_ITEM)
                throw new DomainException($"Máximo de {MAX_UNIDADES_ITEM} unidades por pedido");

            if (item.Quantidade > MAX_UNIDADES_ITEM)
                throw new DomainException($"Máximo de {MAX_UNIDADES_ITEM} unidades por produto");
        }

        public static class PedidoFactory
        {
            public static Pedido NovoPedidoRascunho(Guid clienteId)
            {
                var pedido = new Pedido
                {
                    ClienteId = clienteId,
                };

                pedido.TornarRaschunho();

                return pedido;
            }
        }
    }
}