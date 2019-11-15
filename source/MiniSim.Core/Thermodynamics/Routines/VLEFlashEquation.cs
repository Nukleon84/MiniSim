using MiniSim.Core.Expressions;
using MiniSim.Core.Flowsheeting;
using System;
using System.Collections.Generic;
using System.Linq;

using System.Text;
using System.Threading.Tasks;

namespace MiniSim.Core.Thermodynamics.Routines
{
    public class VLEFlashEquation : Expression
    {
        MaterialStream _stream;

        Expression LV;
        Expression L;
        Expression V;

        int _checkPhaseStateChangeFrequency = 3;
        int _iterationsSinceLastPhaseChange;

        public VLEFlashEquation(MaterialStream stream) : base("FlashZ", () => 1, (vari) => 1)
        {
            _stream = stream;
            ValueFunc = () => Eval();
            DiffFunc = (vari) => Derivative(vari);

            //Check phase state already at first iteration, in case of changed specifications without re-init
            _iterationsSinceLastPhaseChange = 0;

            var x = _stream.Liquid.ComponentMolarFraction;
            var y = _stream.Vapor.ComponentMolarFraction;

            LV = Sym.Sum(x) - Sym.Sum(y);
            V = Sym.Sum(y) - 1;
            L = Sym.Sum(x) - 1;

            this.AddChildren(LV);
            this.AddChildren(V);
            this.AddChildren(L);           
        }

        double LiquidVaporFactor()
        {
            if (_stream.State == PhaseState.LiquidVapor || _stream.State == PhaseState.BubblePoint || _stream.State == PhaseState.DewPoint)
                return 1.0;
            else
                return 0;
        }

        double LiquidFactor()
        {
            if (_stream.State == PhaseState.Liquid)
                return 1.0;
            else
                return 0;
        }

        double VaporFactor()
        {
            if (_stream.State == PhaseState.Vapor)
                return 1.0;
            else
                return 0;
        }

        double Eval()
        {
            _iterationsSinceLastPhaseChange--;


            if (_iterationsSinceLastPhaseChange <= 0)
            {
                var newState = _stream.UpdatePhaseState();
                _iterationsSinceLastPhaseChange = _checkPhaseStateChangeFrequency;
            }

            double rval = 0;

            if (_stream.State == PhaseState.LiquidVapor || _stream.State == PhaseState.BubblePoint || _stream.State == PhaseState.DewPoint)
                rval = LV.Val();

            if (_stream.State == PhaseState.Liquid)
                rval = L.Val();

            if (_stream.State == PhaseState.Vapor)
                rval = V.Val();

           

            return rval;
        }

        double Derivative(Variable vari)
        {
            double rval = 0;

            if (_stream.State == PhaseState.LiquidVapor || _stream.State == PhaseState.BubblePoint || _stream.State == PhaseState.DewPoint)
                rval = LV.Diff(vari);

            if (_stream.State == PhaseState.Liquid)
                rval = L.Diff(vari);

            if (_stream.State == PhaseState.Vapor)
                rval = V.Diff(vari);

            return rval;
        }
    }


}
