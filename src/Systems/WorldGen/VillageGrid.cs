using System;
using System.Collections.Generic;
using System.Text;
using Vintagestory.API.MathTools;

namespace VsVillage
{
    public class VillageGrid
    {
        public EnumgGridSlot[][] grid;

        public List<WorldGenVillageStructure> structures = new List<WorldGenVillageStructure>();

        public int capacity = 16;

        public int width = 9;
        public int height = 9;

        public VillageGrid()
        {
            grid = new EnumgGridSlot[9][];
            for (int i = 0; i < 9; i++)
            {
                grid[i] = new EnumgGridSlot[] { EnumgGridSlot.EMPTY, EnumgGridSlot.EMPTY, EnumgGridSlot.EMPTY, EnumgGridSlot.EMPTY, EnumgGridSlot.EMPTY, EnumgGridSlot.EMPTY, EnumgGridSlot.EMPTY, EnumgGridSlot.EMPTY, EnumgGridSlot.EMPTY };
            }
        }

        public bool BigSlotAvailable()
        {
            return grid[0][0] == EnumgGridSlot.EMPTY;
        }

        public bool MediumSlotAvailable(int x, int y)
        {
            return grid[x * 4 + 1][y * 4 + 1] == EnumgGridSlot.EMPTY;
        }

        public bool SmallSlotAvailable(int x, int y)
        {
            return grid[x * 2 + 1][y * 2 + 1] == EnumgGridSlot.EMPTY;
        }

        public void AddBigStructure(WorldGenVillageStructure structure)
        {
            capacity -= 16;
            structures.Add(structure);
            for (int i = 1; i < 8; i++)
            {
                for (int k = 1; k < 8; k++)
                {
                    grid[i][k] = EnumgGridSlot.STRUCTURE;
                }
            }
            switch (structure.AttachmentPoint)
            {
                case 0:
                    grid[4][8] = EnumgGridSlot.STREET;
                    break;
                case 1:
                    grid[8][4] = EnumgGridSlot.STREET;
                    break;
                case 2:
                    grid[4][0] = EnumgGridSlot.STREET;
                    break;
                case 3:
                    grid[0][4] = EnumgGridSlot.STREET;
                    break;
            }
        }

        public void AddMediumStructure(WorldGenVillageStructure structure, int x, int y)
        {
            capacity -= 4;
            structures.Add(structure);
            for (int i = 0; i < 3; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    grid[x * 4 + 1 + i][y * 4 + 1 + k] = EnumgGridSlot.STRUCTURE;
                }
            }
            switch (structure.AttachmentPoint)
            {
                case 0:
                    grid[x * 4 + 2][y * 4 + 4] = EnumgGridSlot.STREET;
                    break;
                case 1:
                    grid[x * 4 + 4][y * 4 + 2] = EnumgGridSlot.STREET;
                    break;
                case 2:
                    grid[x * 4 + 2][y * 4] = EnumgGridSlot.STREET;
                    break;
                case 3:
                    grid[x * 4][y * 4 + 2] = EnumgGridSlot.STREET;
                    break;
            }
        }

        public void AddSmallStructure(WorldGenVillageStructure structure, int x, int y)
        {
            capacity -= 1;
            structures.Add(structure);
            grid[x * 2 + 1][y * 2 + 1] = EnumgGridSlot.STRUCTURE;
            switch (structure.AttachmentPoint)
            {
                case 0:
                    grid[x * 2 + 1][y * 2 + 2] = EnumgGridSlot.STREET;
                    break;
                case 1:
                    grid[x * 2 + 2][y * 2 + 1] = EnumgGridSlot.STREET;
                    break;
                case 2:
                    grid[x * 2 + 1][y * 2] = EnumgGridSlot.STREET;
                    break;
                case 3:
                    grid[x * 2][y * 2 + 1] = EnumgGridSlot.STREET;
                    break;
            }
        }

        //always go from biggest to smallest structure, otherwise this might break
        public bool tryAddStructure(WorldGenVillageStructure structure, Random random)
        {
            switch (structure.Size)
            {
                case EnumVillageStructureSize.LARGE:
                    if (capacity < 16) { return false; }
                    else { AddBigStructure(structure); return true; }
                case EnumVillageStructureSize.MEDIUM:
                    if (capacity < 4) { return false; }
                    else
                    {
                        var free = new List<Vec2i>();
                        for (int i = 0; i < 2; i++)
                        {
                            for (int k = 0; k < 2; k++)
                            {
                                if (MediumSlotAvailable(i, k))
                                {
                                    free.Add(new Vec2i(i, k));
                                }
                            }
                        }
                        var xy = free[random.Next(0, free.Count)];
                        AddMediumStructure(structure, xy.X, xy.Y);
                        return true;
                    }
                case EnumVillageStructureSize.SMALL:
                    if (capacity < 1) { return false; }
                    else
                    {
                        var free = new List<Vec2i>();
                        for (int i = 0; i < 4; i++)
                        {
                            for (int k = 0; k < 4; k++)
                            {
                                if (SmallSlotAvailable(i, k))
                                {
                                    free.Add(new Vec2i(i, k));
                                }
                            }
                        }
                        var xy = free[random.Next(0, free.Count)];
                        AddSmallStructure(structure, xy.X, xy.Y);
                        return true;
                    }
                default: return false;

            }
        }

        public void connectStreets()
        {
            var connectedStreets = new List<Vec2i>();
            int currentX = 0;
            int currentY = 0;
            for (int i = 0; i < 9 * 9; i++)
            {
                if (grid[currentX][currentY] == EnumgGridSlot.STREET)
                {
                    addStreedToGrid(connectedStreets, new Vec2i(currentX, currentY));
                }
                currentX--;
                currentY++;
                if (currentX < 0 && currentY < 9)
                {
                    currentX = currentY;
                    currentY = 0;
                }
                else if (currentY >= 9)
                {
                    currentY = currentX + 2;
                    currentX = 8;
                }
            }
        }

        private void addStreedToGrid(List<Vec2i> connectedStreets, Vec2i newStreet)
        {
            if (connectedStreets.Count == 0)
            {
                connectedStreets.Add(newStreet);
            }
            else
            {
                // get closest street
                var nearest = connectedStreets[0];
                var nearestDistance = Math.Abs(newStreet.X - nearest.X) + Math.Abs(newStreet.Y - nearest.Y);
                foreach (var candidate in connectedStreets)
                {
                    var candidateDistance = Math.Abs(newStreet.X - candidate.X) + Math.Abs(newStreet.Y - candidate.Y);
                    if (candidateDistance < nearestDistance)
                    {
                        nearest = candidate;
                        nearestDistance = candidateDistance;
                    }
                }
                // conntect streets
                int currentX = nearest.X;
                int currentY = nearest.Y;
                bool canWalkY;
                bool canWalkX;
                bool canWalkTowards = true;
                int directionX = Math.Sign(newStreet.X - currentX + 0.5f);
                int directionY = Math.Sign(newStreet.Y - currentY + 0.5f);
                bool? goHorizontal = null;
                while (Math.Abs(newStreet.X - currentX) + Math.Abs(newStreet.Y - currentY) > 1)
                {
                    canWalkX = currentY % 2 == 0 && inWidthBounds(currentX + directionX) && grid[currentX + directionX][currentY] == EnumgGridSlot.EMPTY;
                    canWalkY = currentX % 2 == 0 && inHeightBounds(currentY + directionY) && grid[currentX][currentY + directionY] == EnumgGridSlot.EMPTY;
                    canWalkTowards &= canWalkX && (newStreet.X - currentX) * directionX > 0 || canWalkY && (newStreet.Y - currentY) * directionY > 0;
                    if (!canWalkTowards)
                    {
                        if (goHorizontal == null)
                        {
                            goHorizontal = canWalkX;
                        }

                        if (canWalkY && goHorizontal == true || canWalkX && goHorizontal == false)
                        {
                            if (goHorizontal == true) { currentY += directionY; }
                            else { currentX += directionX; }
                            goHorizontal = null;
                            canWalkTowards = true;
                            directionX = Math.Sign(newStreet.X - currentX + 0.5f);
                            directionY = Math.Sign(newStreet.Y - currentY + 0.5f);
                        }
                        else if (goHorizontal == true) { currentX += directionX; }
                        else { currentY += directionY; }
                    }
                    else if (canWalkX && Math.Abs(newStreet.X - currentX) > Math.Abs(newStreet.Y - currentY) || !canWalkY)
                    {
                        currentX += directionX;
                    }
                    else
                    {
                        currentY += directionY;
                    }
                    grid[currentX][currentY] = EnumgGridSlot.STREET;
                    connectedStreets.Add(new Vec2i(currentX, currentY));
                }
            }
        }

        public string debugPrintGrid()
        {
            var sb = new StringBuilder();
            for (int i = 0; i < 9; i++)
            {
                for (int k = 0; k < 9; k++)
                {
                    sb.Append((int)grid[k][8 - i]).Append(" ");
                }
                sb.Append("\n");
            }
            return sb.ToString();
        }

        private bool inHeightBounds(int y)
        {
            return y >= 0 && y < height;
        }

        private bool inWidthBounds(int x)
        {
            return x >= 0 && x < width;
        }
    }

    public enum EnumgGridSlot
    {
        EMPTY, STRUCTURE, STREET
    }
}