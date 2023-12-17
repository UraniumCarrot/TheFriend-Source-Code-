namespace TheFriend.NoirThings;

public partial class NoirCatto
{
    #region DirectlyToBeam
    private static void DirectlyToBeam1(NoirData noirData, out bool flag)
    {
        var self = noirData.Cat;
        var graphics = (PlayerGraphics)self.graphicsModule;
        
        var anyPartOnVertBeam = (self.room.GetTile(self.room.GetTilePosition(self.bodyChunks[0].pos)).verticalBeam || self.room.GetTile(self.room.GetTilePosition(self.bodyChunks[1].pos)).verticalBeam ||
                                 self.room.GetTile(self.room.GetTilePosition(graphics.hands[0].pos)).verticalBeam || self.room.GetTile(self.room.GetTilePosition(graphics.hands[1].pos)).verticalBeam);
        
        var anyPartOnHoriBeam = (self.room.GetTile(self.room.GetTilePosition(self.bodyChunks[0].pos)).horizontalBeam || self.room.GetTile(self.room.GetTilePosition(self.bodyChunks[1].pos)).horizontalBeam ||
                                 self.room.GetTile(self.room.GetTilePosition(graphics.hands[0].pos)).horizontalBeam || self.room.GetTile(self.room.GetTilePosition(graphics.hands[1].pos)).horizontalBeam);
        
        if (!anyPartOnVertBeam && 
            (self.animation == Player.AnimationIndex.StandOnBeam || noirData.LastAnimation == Player.AnimationIndex.StandOnBeam || 
             self.animation == Player.AnimationIndex.GetUpOnBeam || noirData.LastAnimation == Player.AnimationIndex.GetUpOnBeam))
        {
            if (self.input[0].y > 0)
            {
                self.input[0].y = 0;
            }
        }
        
        if (!anyPartOnVertBeam && noirData.LastJumpFromHorizontalBeam && 
            self.mainBodyChunk.lastPos.y <= self.mainBodyChunk.pos.y &&
            (anyPartOnHoriBeam || self.animation == Player.AnimationIndex.StandOnBeam || noirData.LastAnimation == Player.AnimationIndex.StandOnBeam) )
        {
            
            for (var i = 0; i < self.input.Length; i++)
            {
                if (self.input[0].jmp || (self.input[i].jmp && !self.input[0].jmp))
                {
                    self.input[0].y = 0;
                    flag = true;
                }
            }
        }
        flag = false;
    }

    private static void DirectlyToBeam2(NoirData noirData, bool flag)
    {
        var self = noirData.Cat;
        if (self.animation == Player.AnimationIndex.HangFromBeam && noirData.LastAnimation != Player.AnimationIndex.HangFromBeam &&
            noirData.LastAnimation != Player.AnimationIndex.StandOnBeam &&
            self.input[0].y > 0 && self.mainBodyChunk.lastPos.y >= self.mainBodyChunk.pos.y
            && !flag && noirData.CanGrabBeam())
        {
            // Grab the beam here
            noirData.YinputForPoleBlocker = 10;

            // Debug.Log($"tile: {self.room.MiddleOfTile(self.bodyChunks[1].pos).y}");
            // Debug.Log($"pos: {self.bodyChunks[1].pos.y}");

            if (self.bodyChunks[1].pos.y < self.bodyChunks[0].pos.y)
            {
                self.bodyChunks[1].pos.y += 7f;
            }
            else
            {
                self.bodyChunks[1].pos.y -= 14f;
                self.bodyChunks[0].pos.y += 7f;
            }

            self.bodyChunks[0].vel.y = 0.0f;
            self.bodyChunks[1].vel.y = 0.0f;
            self.animation = Player.AnimationIndex.StandOnBeam;
        }
    }
    #endregion

    private static void PoleLeapUpdate(NoirData noirData)
    {
        var self = noirData.Cat;

        if (!noirData.CanCrawlOnBeam() || !self.input[0].jmp)
        {
            if (noirData.SuperCrawlPounce > 0)
            {
                noirData.SuperCrawlPounce--;
            }
        }

        ModifyPoleLeapInput(noirData);

        if (noirData.CanCrawlOnBeam() && noirData.SuperCrawlPounce >= 19 && !noirData.UnchangedInput[0].jmp && noirData.UnchangedInput[1].jmp)
        {
            self.Jump();
        }
    }
}