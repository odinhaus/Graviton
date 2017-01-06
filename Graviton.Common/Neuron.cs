using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graviton.Common
{
    public class Neuron
    {
        public const float MinPotential = -0.5f;
        public const float DecayRate = -0.012f;
        public Neuron()
        {
            Dendrites = new List<Connection>();
            Axon = new List<Connection>();
            Decay = (date, potential) => potential * (float)Math.Exp(DecayRate * date.Subtract(LastSignaled).TotalMilliseconds);
        }
        public float Potential;
        public float Threshold = 0.75f;
        public DateTime LastSignaled;
        public List<Connection> Dendrites;
        public List<Connection> Axon;
        public Func<DateTime, float, float> Decay;

        public void Update(DateTime time)
        {
            Potential = Decay(time, Potential);
            Connection dendrite;
            float totalWeight = 0f;
            float currentWeight = 0f;
            for(int i = 0; i < Dendrites.Count; i++)
            {
                dendrite = Dendrites[i];
                if (dendrite.Sign > 0)
                    totalWeight += dendrite.Weight;

                if (dendrite.IsSignaled)
                {
                    currentWeight += dendrite.Weight * dendrite.Sign;
                }
            }

            Potential += currentWeight / totalWeight;

            if (Potential >= Threshold)
            {
                LastSignaled = time;
                Connection axon;
                for(int i = 0; i < Axon.Count; i++)
                {
                    axon = Axon[i];
                    axon.IsSignaled = true;
                    axon.LastSignaled = time;
                }

                Potential = MinPotential;
            }
        }
    }
}
