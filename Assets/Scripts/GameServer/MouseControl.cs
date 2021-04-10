using Assets.Scripts.Shogi;

namespace Assets.Scripts.GameServer {
    public class MouseControl {
        // 盤面の状態
        public MouseStateEnum State;

        // マウスの入力先
        public SquareHand PickedFrom;
        public SquareHand PickedTo;

        // ダイアログの選択
        public PromoteDialogSelectEnum Select;
    };

    public enum MouseStateEnum {
        // 何もつかんでいない
        None,

        // 駒をつかんでいる
        Picked,

        // ダイアログ出現中
        PromoteDialog,
    };

    public enum PromoteDialogSelectEnum {
        None,
        Promote,
        NonPromote,
    };
}
