using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.Shell;

using RipTrail.ViewModel;

namespace RipTrail.ViewModel
{

    class FlipTile
    {
        private readonly UnitConversions _unitConversions;

        public FlipTile()
        {
             UnitConversions _unitConversions = new UnitConversions();


        }

        FlipTileData Tile = new FlipTileData()
        {
           Title = "Rip Trail",
           BackTitle = "Rip Trail",
           BackContent = "[back of medium Tile size content]",
           WideBackContent = "[back of wide Tile size content]",
         
           //SmallBackgroundImage = [small Tile size URI],
           //BackgroundImage = [front of medium Tile size URI],
           //BackBackgroundImage = [back of medium Tile size URI],
           //WideBackgroundImage = [front of wide Tile size URI],
           //WideBackBackgroundImage = [back of wide Tile size URI],
        };
    }
}
