using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Match
{
    public List<Vector3Int> MatchingBlocks = new List<Vector3Int>();
    public Vector3Int MainPos;

    public Block_Special specialBlock = null;

    public void AddMatchingBlock(Block _block)
    {
        if (_block.curMatch != null)
            return;

        MatchingBlocks.Add(_block.boardPos);
        _block.curMatch = this;

    }
}
