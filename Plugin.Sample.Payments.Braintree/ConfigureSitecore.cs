﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfigureSitecore.cs" company="Sitecore Corporation">
//   Copyright (c) Sitecore Corporation 1999-2017
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Plugin.Sample.Payments.Braintree
{
    using System.Reflection;
    using Microsoft.Extensions.DependencyInjection;
    using Sitecore.Commerce.Core;
    using Sitecore.Commerce.Plugin.Orders;
    using Sitecore.Commerce.Plugin.Payments;
    using Sitecore.Framework.Configuration;
    using Sitecore.Framework.Pipelines.Definitions.Extensions;

    /// <summary>
    /// The pricing configure sitecore class
    /// </summary>
    /// <seealso cref="Sitecore.Framework.Configuration.IConfigureSitecore" />
    public class ConfigureSitecore : IConfigureSitecore
    {
        /// <summary>
        /// The configure services.
        /// </summary>
        /// <param name="services">
        /// The services.
        /// </param>
        public void ConfigureServices(IServiceCollection services)
        {
            var assembly = Assembly.GetExecutingAssembly();
            services.RegisterAllPipelineBlocks(assembly);

            services.Sitecore().Pipelines(config => config               

                .ConfigurePipeline<IGetClientTokenPipeline>(d =>
                {
                    d.Add<GetClientTokenBlock>().After<Sitecore.Commerce.Plugin.Payments.GetClientTokenBlock>();
                })
                .ConfigurePipeline<ICreateOrderPipeline>(d =>
                {
                    d.Add<CreateFederatedPaymentBlock>().Before<CreateOrderBlock>();
                })
                .ConfigurePipeline<IReleaseOnHoldOrderPipeline>(d =>
                {
                    d.Add<UpdateFederatedPaymentBlock>().After<ValidateOnHoldOrderBlock>();
                })
                .ConfigurePipeline<ISettleSalesActivityPipeline>(d =>
                {
                    d.Add<EnsureSettlePaymentRequestedBlock>().After<ValidateSalesActivityBlock>()
                     .Add<ValidateSettlementBlock>().After<EnsureSettlePaymentRequestedBlock>()
                     .Add<UpdateFederatedPaymentAfterSettlementBlock>().Before<PersistSalesActivityBlock>();
                })               
                .ConfigurePipeline<IRefundPaymentsPipeline>(d =>
                {
                    d.Add<RefundFederatedPaymentBlock>().After<RefundCreditCardPaymentBlock>();
                })               
                .ConfigurePipeline<ICancelOrderPipeline>(d =>
                {
                    d.Add<VoidCancelOrderFederatedPaymentBlock>().After<GetPendingOrderBlock>();
                })

            );

            services.RegisterAllCommands(assembly);
        }
    }
}
