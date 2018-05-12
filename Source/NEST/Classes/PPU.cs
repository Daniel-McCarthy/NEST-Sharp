using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NEST.Classes
{
    class PPU
    {
        private byte[] ppuRam = new byte[0x4000];

        private byte getPPURegister()
        {
            return Core.cpu.readCPURam(0x2000, true);
        }

        private byte getPPURegisterTableSetting()
        {
            //Base Name Table Address Setting
            //0: 0x2000, 1: 0x2400, 2: 0x2800, 3: 0x2C00
            return (byte)(getPPURegister() & 0b00000111);
        }

        private bool getPPURegisterSpritePatternTableSetting()
        {
            //Pattern Table address for 8x8 sprites
            //0: 0x0000, 1: 0x1000
            return (getPPURegister() & 0b0001000) != 0;
        }

        private bool getPPURegisterBackgroundPatternTableSetting()
        {
            //Pattern Table address for background
            //0: 0x0000, 1: 0x1000
            return (getPPURegister() & 0b00100000) != 0;
        }

        private bool getPPURegisterSpriteSizeSetting()
        {
            //Sprite size Setting
            //0: 8x8 Sprites, 1: 8x16 Sprites
            return (getPPURegister() & 0b01000000) != 0;
        }

    }
}
