using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITProject.Model
{
    public class DamageNumber
    {
        public System.Numerics.Vector2 Position;

        public int Value;
        public bool Remove = false;

        private double _displayedTime;
        private double _maxDisplayedTime = 1d;
        private float _movingOffset = 0.05f;

        public DamageNumber(System.Numerics.Vector2 position, int value)
        {
            Position = position;
            Value = value;
        }

        public void Update(double deltaTime)
        {
            _displayedTime += deltaTime;
            Position.Y += _movingOffset;

            if (_displayedTime > _maxDisplayedTime) Remove = true;
        }
    }
}
