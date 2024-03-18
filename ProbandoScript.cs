#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
using System.Security.AccessControl;
#endregion

//This namespace holds Strategies in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Strategies
{
	public class ProbandoScriptDos : Strategy
	{
		private RSI rsi;
		private EMA ema;
		private ADX adx;

		private bool BreakEvenOn;
		private double BETrigger;


        protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Primer robot escrito desde cero.";
				Name										= "Prueba Script Coding";
				Calculate									= Calculate.OnPriceChange;
				EntriesPerDirection							= 1;
				EntryHandling								= EntryHandling.AllEntries;
				IsExitOnSessionCloseStrategy				= true;
				ExitOnSessionCloseSeconds					= 600;
				IsFillLimitOnTouch							= false;
				MaximumBarsLookBack							= MaximumBarsLookBack.TwoHundredFiftySix;
				OrderFillResolution							= OrderFillResolution.Standard;
				Slippage									= 0;
				StartBehavior								= StartBehavior.WaitUntilFlat;
				TimeInForce									= TimeInForce.Gtc;
				TraceOrders									= false;
				RealtimeErrorHandling						= RealtimeErrorHandling.StopCancelClose;
				StopTargetHandling							= StopTargetHandling.PerEntryExecution;
				BarsRequiredToTrade							= 20;
				// Disable this property for performance gains in Strategy Analyzer optimizations
				// See the Help Guide for additional information
				IsInstantiatedOnEachOptimizationIteration	= true;
				RsiPeriods									= 14;
                RsiMedia									= 3;
				RsiUpper 									= 60;
				RsiLower 									= 35;
				BbPeriods									= 30;
				BBStdv										= 2;
				EMAPeriods									= 100;
				StopLossDistance							= 8;
				TakeProfitDistance							= 12;
				BEDistance									= 4;
				BEOffset									= 2;
				OrderSize									= 1;
				horaInicio									= ToTime(9, 00, 00);
                horaFin										= ToTime(12, 30, 00);
                Inicio = DateTime.Parse("21:30", System.Globalization.CultureInfo.InvariantCulture);
                Fin = DateTime.Parse("21:30", System.Globalization.CultureInfo.InvariantCulture);

            }
			else if (State == State.Configure)
			{
			}else if (State == State.DataLoaded)
			{				
				ema = EMA(EMAPeriods);
				ema.Plots[0].Brush = Brushes.Snow;
				AddChartIndicator(ema);
				
				rsi = RSI(Close, Convert.ToInt32(RsiPeriods), 3);
				rsi.Plots[0].Brush = Brushes.Snow;
				rsi.Plots[1].Brush = Brushes.Transparent;						
				AddChartIndicator(rsi);
			}
		}

		protected override void OnBarUpdate()
		{	
			
			BreakEvenOn = false;
			if(Position.MarketPosition == MarketPosition.Flat){
				BreakEvenOn = false;
			}
			
			//Comprando despues de sobreventa
			if(CrossAbove(rsi, Convert.ToInt32(RsiLower), 1) && FiltroEMA() == "ALCISTA" && FiltroHorario())
            {
				Print("RSI Cruzando por encima de "+RsiLower);
				Print(Time[0]);
				EnterLong(Convert.ToInt32(OrderSize), @"COMPRA");
            }

			//SL Y TP COMPRA
			if(Position.MarketPosition == MarketPosition.Long){
                BreakEvenOn = false;
                ExitLongLimit(Position.AveragePrice + TakeProfitDistance * TickSize, @"TP_COMPRA", @"COMPRA"); //Seteamos TP
                ExitLongStopMarket(Position.AveragePrice - StopLossDistance * TickSize,  @"SL_COMPRA", @"COMPRA"); //Seteamos SL
                BETrigger = (Position.AveragePrice + BEDistance * TickSize); //Seteamos preico a BE
            }

			
			//Vendiendo despues de sobrecompra
			if(CrossBelow(rsi, Convert.ToInt32(RsiUpper), 1) && FiltroEMA() == "BAJISTA" && FiltroHorario())
            {
				Print("RSI Cruzando por debajo de "+ RsiUpper);
				Print(Time[0]);
				EnterShort(Convert.ToInt32(OrderSize), @"VENTA");   
            }

			//SL Y TP VENTA
			if(Position.MarketPosition == MarketPosition.Short) {
                BreakEvenOn = false;
                ExitShortLimit(Position.AveragePrice - TakeProfitDistance * TickSize, @"TP_VENTA", @"VENTA"); //Seteamos TP
                ExitShortStopMarket(Position.AveragePrice + StopLossDistance * TickSize, @"SL_VENTA", @"VENTA"); //Seteamos SL
                BETrigger = (Position.AveragePrice - BEDistance * TickSize); //Seteamos preico a BE
            }


			//BE COMPRA
			if(Close[0] >= BETrigger) {
				BreakEvenOn = true;
                ExitLongStopMarket(Position.AveragePrice + BEOffset * TickSize, @"BE_COMPRA", @"COMPRA"); //Activamos BE
            }

			//BE VENTA
			if (Close[0] <= BETrigger) {
                BreakEvenOn = true;
                ExitShortStopMarket(Position.AveragePrice - BEOffset * TickSize, @"BE_VENTA", @"VENTA"); //Activamos BE
            }

			//SALVAVIDAS COMPRA
			if(Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks) <= -5 && Position.MarketPosition == MarketPosition.Long) {
				ExitLong(@"SALVAVIDAS_COMPRA", @"COMPRA");
            }

			//SALVAVIDAS VENTA
            if (Position.GetUnrealizedProfitLoss(PerformanceUnit.Ticks) <= -5 && Position.MarketPosition == MarketPosition.Short)
            {
                ExitLong(@"SALVAVIDAS_VENTA",@"VENTA");
            }

        }

        private string FiltroEMA(){
			if (Close[1] > ema[1]) { 
				return "ALCISTA";
			}else return "BAJISTA";
        }

		private bool FiltroHorario() {
			if (DateTime.UtcNow >= Inicio && DateTime.UtcNow <= Fin) {
				return true;
			}return false;
		}

        #region Properties
        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Rsi Periods", Order=1, GroupName="Rsi Parameters")]
		public int RsiPeriods
		{ get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Rsi Mean", Order = 2, GroupName = "Rsi Parameters")]
        public int RsiMedia
        { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Rsi Upper", Order = 3, GroupName = "Rsi Parameters")]
        public int RsiUpper
        { get; set; }
		
		[NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Rsi Lower", Order = 4, GroupName = "Rsi Parameters")]
        public int RsiLower
        { get; set; }

        [NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BB Periods", Order=3, GroupName="Bollinger Parameters")]
		public int BbPeriods
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BB Stdv", Order=4, GroupName="Bollinger Parameters")]
		public int BBStdv
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="EMA Periods", Order=5, GroupName="Trend Identification")]
		public int EMAPeriods
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StopLoss Distance", Order=4, GroupName="Risk Management")]
		public int StopLossDistance
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="TakeProfit Distance", Order=6, GroupName="Risk Management")]
		public int TakeProfitDistance
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BE Distance", Order=7, GroupName="Risk Management")]
		public int BEDistance
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="BE Offset", Order=8, GroupName="Risk Management")]
		public int BEOffset
		{ get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "Order Size", Order = 8, GroupName = "Risk Management")]
        public int OrderSize
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "HoraInicio", Order = 8, GroupName = "Risk Management")]
        public int horaInicio
        { get; set; }

        [NinjaScriptProperty]
        [Range(1, int.MaxValue)]
        [Display(Name = "HoraFin", Order = 8, GroupName = "Risk Management")]
        public int horaFin
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Inicio", Description = "Hora de inicio", Order = 1, GroupName = "Parameters")]
        public DateTime Inicio
        { get; set; }

        [NinjaScriptProperty]
        [PropertyEditor("NinjaTrader.Gui.Tools.TimeEditorKey")]
        [Display(Name = "Fin", Description = "Hora Final", Order = 2, GroupName = "Parameters")]
        public DateTime Fin
        { get; set; }
        #endregion

    }
}
