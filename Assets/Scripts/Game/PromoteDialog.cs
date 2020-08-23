using UnityEngine;
using Assets.Scripts.Shogi;

namespace Assets.Scripts.Game
{
    public class PromoteDialog : MonoBehaviour
    {
        [SerializeField]
        public LayerMask mask_ = default;

        private BoardManager boardManager_;

        public void PutDialog(Piece Pc, Vector3 toPos)
        {
            boardManager_ = GetComponentInParent<BoardManager>();
            
            var ProPc = Pc.ToInt() + Piece.PROMOTE;
            bool IsBlack = Pc.PieceColor() == Shogi.Color.BLACK;

            transform.position =
#if false
            new Vector3(toPos.x, (IsBlack ? toPos.y - BoardConst.PIECE_Y : toPos.y + BoardConst.PIECE_Y), 0)
#else
            new Vector3(toPos.x, toPos.y, 0)
#endif
            ;

            var Left  = transform.GetChild(0).GetComponent<SpriteRenderer>();
            var Right = transform.GetChild(1).GetComponent<SpriteRenderer>();

            Left.sprite = boardManager_.GetSprite(ProPc.Type());
            Right.sprite = boardManager_.GetSprite(Pc.Type());
            Left.flipX = Left.flipY = Right.flipX = Right.flipY = !IsBlack;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Collider2D collider = Physics2D.OverlapPoint(pos, mask_);

                if (collider == null)
                    return;

                if (collider.name == "Pro")
                    boardManager_.BoardController().promoteDialogSelect = PromoteDialogSelect.Promote;
                else if (collider.name == "NonPro")
                    boardManager_.BoardController().promoteDialogSelect = PromoteDialogSelect.NonPromote;
                else
                    return;

                Destroy(gameObject);
            }
        }
    }
}