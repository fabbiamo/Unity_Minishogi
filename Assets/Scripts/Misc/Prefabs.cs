using Assets.Scripts.Shogi;
using UnityEngine;
using SColor = Assets.Scripts.Shogi.Color;

namespace Assets.Scripts.Misc
{
    public class Prefabs : MonoBehaviour
    {
        static GameObject piecePrefab;
        static GameObject promoteDialogPrefab;
        static GameObject[] pieces = new GameObject[(int)PieceNo.NB];

        public static void Load()
        {
            piecePrefab = Resources.Load("Piece") as GameObject;
            promoteDialogPrefab = Resources.Load("PromoteDialog") as GameObject;
        }

        public static void Clear()
        {
            foreach (var piece in pieces)
                Destroy(piece);
        }

        public static void PutPiece(Square sq, Piece pc, PieceNo pn, SColor us, Transform parent)
        {
            var pieceObject = Instantiate(piecePrefab, parent);
            pieceObject.transform.position = PositionConst.SquareToPosition(sq);

            var sr = pieceObject.GetComponent<SpriteRenderer>();
            sr.sprite = SpriteManager.GetSprite(pc.Type());
            sr.flipX = sr.flipY = pc.PieceColor() == SColor.WHITE;

            var boxCollider = pieceObject.GetComponent<BoxCollider2D>();
            boxCollider.enabled = us == SColor.NB || us == pc.PieceColor();
            pieces[(int)pn] = pieceObject;
        }

        public static void PutPromotedialog(Square sq, Piece pc, Transform parent)
        {
            var pos = PositionConst.SquareToPosition(sq);
            var dialogObject = Instantiate(promoteDialogPrefab, parent);
            dialogObject.transform.position = new Vector3(pos.x, pos.y, 0);

            var lsr = dialogObject.transform.GetChild(0).GetComponent<SpriteRenderer>();
            var rsr = dialogObject.transform.GetChild(1).GetComponent<SpriteRenderer>();
            lsr.sprite = SpriteManager.GetSprite((pc.ToInt() + Piece.PROMOTE).Type());
            rsr.sprite = SpriteManager.GetSprite(pc.Type());
            lsr.flipX = lsr.flipY = rsr.flipX = rsr.flipY = pc.PieceColor() != SColor.BLACK;
        }

        public static void MovePiece(SquareHand sq, Piece pc, PieceNo pn, bool newsprite)
        {
            var pickedFrom = pieces[(int)pn];
            if (newsprite)
                pickedFrom.GetComponent<SpriteRenderer>().sprite = SpriteManager.GetSprite(pc.Type());

            // 駒をsqに移動
            pickedFrom.transform.position = PositionConst.SquareToPosition(sq);
        }

        public static void CapturePiece(SquareHand sq, Piece pt, PieceNo pn, bool box2d_enable)
        {
            var pickedTo = pieces[(int)pn];
            var sr = pickedTo.GetComponent<SpriteRenderer>();
            sr.sprite = SpriteManager.GetSprite(pt);
            sr.flipX = sr.flipY = !sr.flipX;

            pickedTo.transform.position = PositionConst.SquareToPosition(sq);
            pickedTo.GetComponent<BoxCollider2D>().enabled = box2d_enable;
        }
    }
}
