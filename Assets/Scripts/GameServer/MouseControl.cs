using Assets.Scripts.Shogi;

namespace Assets.Scripts.GameServer
{
    public class MouseControl
    {
        // 盤面の状態
        public ScreenStateEnum state;

        // マウスの入力先
        public SquareHand pickedFrom;
        public SquareHand pickedTo;

        // ダイアログの選択
        public PromoteDialogSelectEnum select;
    };

    public enum ScreenStateEnum
    {
        // 何もつかんでいない
        None,

        // 駒をつかんでいる
        Picked,

        // ダイアログ出現中
        PromoteDialog,
    };

    public enum PromoteDialogSelectEnum
    {
        None,
        Promote,
        NonPromote,
    };
}
