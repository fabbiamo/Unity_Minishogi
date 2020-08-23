using System;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Shogi
{
    public class Position
    {
        public class StateInfo
        {
            /// <summary>
            /// 現在の局面のhash key
            /// </summary>
            public HASH_KEY key;

            /// <summary>
            /// 連続王手の千日手の検出に必要
            /// </summary>
            public int[] continuousCheck = new int[(int)Color.NB];

            /// <summary>
            /// 直前の指し手
            /// </summary>
            public Move lastMove;

            /// <summary>
            /// 現局面で手番側に対して王手をしている駒のbitboard
            /// </summary>
            public Bitboard checkersBB;

            /// <summary>
            /// 自玉に対して(敵駒によって)pinされている駒
            /// </summary>
            public Bitboard[] blockersForKing = new Bitboard[(int)Color.NB];

            /// <summary>
            /// 自玉に対してpinしている(可能性のある)敵の大駒
            /// </summary>
            public Bitboard[] pinnersForKing = new Bitboard[(int)Color.NB];

            /// <summary>
            /// 自駒の駒種Xによって敵玉が王手となる升のbitboard
            /// </summary>
            public Bitboard[] checkSquares = new Bitboard[(int)Piece.WHITE];

            /// <summary>
            /// この局面で捕獲された駒
            /// 先後の区別あり
            /// </summary>
            public Piece capturedPiece;

            /// <summary>
            /// 一手前の局面へのポインタ
            /// previous == null であるとき、これ以上辿れない
            /// これを辿ることで千日手判定などを行う
            /// </summary>
            public StateInfo previous;
        }

        /// <summary>
        /// 盤面
        /// </summary>
        private Piece[] board = new Piece[(int)Square.NB];
        private PieceNo[] board_pn = new PieceNo[(int)Square.NB];

        /// <summary>
        /// 手駒
        /// </summary>
        private Hand[] hands = new Hand[(int)Color.NB];
        private PieceNo[,,] hand_pn = new PieceNo[(int)Color.NB, (int)Piece.HAND_NB, 2];

        // 使用しているPieceNoの終端
        public PieceNo lastPieceNo { get; private set; }

        /// <summary>
        ///  手番
        /// </summary>
        public Color sideToMove { get; private set; } = Color.BLACK;

        /// <summary>
        /// 玉の位置
        /// </summary>
        private Square[] kingSquare = new Square[(int)Color.NB];

        public int gamePly { get; private set; } = 1;

        /// <summary>
        /// 局面の付随情報
        /// </summary>
        private StateInfo st;

        /// <summary>
        /// 局面の付随情報
        /// </summary>
        public StateInfo State() { return st; }

        /// <summary>
        /// 現局面のhash key。
        /// </summary>
        /// <returns></returns>
        public HASH_KEY Key() { return st.key; }

        // 盤上の先手/後手/両方の駒があるところが1であるBitboard
        private Bitboard[] byColorBB = new Bitboard[(int)Color.NB];

        // 駒が存在する升を表すBitboard。先後混在。
        // pieces()の引数と同じく、ALL,HDKなどのPieceで定義されている特殊な定数が使える。
        private Bitboard[] byTypeBB = new Bitboard[(int)Piece.BB_NB];

        public string Pretty()
        {
            var st = new StringBuilder();
            for (Rank r = Rank.RANK_1; r <= Rank.RANK_5; ++r)
            {
                for (File f = File.FILE_5; f >= File.FILE_1; --f)
                    st.Append(board[Util.MakeSquare(f, r).ToInt()].Pretty());

                st.AppendLine();
            }
            return st.ToString();
        }
        public string PrettyPieceNo()
        {
            var sb = new StringBuilder();

            for (Rank r = Rank.RANK_1; r <= Rank.RANK_5; ++r)
            {
                for (File f = File.FILE_5; f >= File.FILE_1; --f)
                {
                    var pn = PieceNoOn(Util.MakeSquare(f, r));
                    sb.Append(string.Format("{0:D2} ", (int)pn));
                }
                sb.AppendLine();
            }

            foreach (var c in All.Colors())
            {
                sb.Append(c.Pretty() + ":");
                for (Piece p = Piece.PAWN; p < Piece.HAND_NB; ++p)
                {
                    int count = Hand(c).Count(p);
                    if (count == 0)
                        continue;

                    sb.Append(p.Pretty());
                    for (int i = 0; i < count; ++i)
                    {
                        var pn = HandPieceNo(c, p, i);
                        sb.Append(string.Format("{0:D2} ", (int)pn));
                    }
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        public void SetHirate()
        {
            st = new StateInfo() { previous = null };

            Array.Clear(board, 0, board.Length);
            Array.Clear(board_pn, 0, board_pn.Length);
            Array.Clear(hands, 0, hands.Length);
            Array.Clear(hand_pn, 0, hand_pn.Length);
            Array.Clear(byColorBB, 0, byColorBB.Length);
            Array.Clear(byTypeBB, 0, byTypeBB.Length);

            lastPieceNo = PieceNo.ZERO;

            PutPiece(Square.SQ_11, Piece.W_KING,   lastPieceNo++);
            PutPiece(Square.SQ_12, Piece.W_PAWN,   lastPieceNo++);
            PutPiece(Square.SQ_21, Piece.W_GOLD,   lastPieceNo++);
            PutPiece(Square.SQ_31, Piece.W_SILVER, lastPieceNo++);
            PutPiece(Square.SQ_41, Piece.W_BISHOP, lastPieceNo++);
            PutPiece(Square.SQ_51, Piece.W_ROOK,   lastPieceNo++);
            PutPiece(Square.SQ_55, Piece.B_KING,   lastPieceNo++);
            PutPiece(Square.SQ_54, Piece.B_PAWN,   lastPieceNo++);
            PutPiece(Square.SQ_45, Piece.B_GOLD,   lastPieceNo++);
            PutPiece(Square.SQ_35, Piece.B_SILVER, lastPieceNo++);
            PutPiece(Square.SQ_25, Piece.B_BISHOP, lastPieceNo++);
            PutPiece(Square.SQ_15, Piece.B_ROOK,   lastPieceNo++);

            sideToMove = Color.BLACK;

            gamePly = 0;

            SetState(st);

            UpdateBitboards();

            SetCheckInfo(st);
        }

        /// <summary>
        /// 指し手で盤面を1手進める
        /// </summary>
        /// <param name="m"></param>
        public void DoMove(Move m)
        {
            Color Us = sideToMove, them = Us.Not();
            Square to = m.To();

            // StateInfoの更新
            var newSt = new StateInfo
            {
                previous = st,
                key = st.key
            };
            st = newSt;

            if (m.IsDrop())
            {
                Piece pr = m.DroppedPiece();
                Piece pc = Util.MakePiece(Us, pr);
                Hand(Us).Sub(pr);

                PieceNo pn = HandPieceNo(Us, pr, Hand(Us).Count(pr));
                
                PutPiece(to, pc, pn);

                // hash keyの更新
                st.key -= Zobrist.Hand(Us, pr);
                st.key += Zobrist.Psq(to, pc);

                st.capturedPiece = Piece.NO_PIECE;
            }
            else
            {
                Square from = m.From();
                PieceNo pn = PieceNoOn(from);
                Piece moved_pc = RemovePiece(from);

                PieceNoOn(from) = PieceNo.NONE;

                if ((Pieces(them) & to).IsNotZero())
                {
                    Piece to_pc = PieceOn(to);
                    Piece pr = to_pc.RawType();

                    PieceNo pn2 = PieceNoOn(to);
                    HandPieceNo(Us, pr, Hand(Us).Count(pr)) = pn2;

                    Hand(Us).Add(pr);
                    
                    // 捕獲された駒が盤上から消えるので局面のhash keyを更新する
                    st.key -= Zobrist.Psq(to, to_pc);
                    st.key += Zobrist.Hand(Us, pr);

                    RemovePiece(to);
                    st.capturedPiece = to_pc;
                }
                else
                {
                    st.capturedPiece = Piece.NO_PIECE;
                }
                
                Piece moved_after_pc = moved_pc + (m.IsPromote() ? Piece.PROMOTE.ToInt() : 0);
                PutPiece(to, moved_after_pc, pn);

                // fromにあったmoved_pcがtoにmoved_after_pcとして移動した。
                st.key -= Zobrist.Psq(from, moved_pc);
                st.key += Zobrist.Psq(to, moved_after_pc);
            }
            sideToMove = them;

            // -- update

            // bitboardを更新
            UpdateBitboards();

            // 王手関係の情報を更新
            SetCheckInfo(st);

            // 直前の指し手の更新
            st.lastMove = m;

            // Zobrist.sideはp1==0が保証されているのでこれで良い
            st.key.p.p0 ^= Zobrist.Side.p.p0;

            gamePly++;
        }

        /// <summary>
        /// 指し手で盤面を1手戻す
        /// </summary>
        public void UndoMove()
        {
            // Usは1手前の局面での手番
            var us = sideToMove.Not();
            var m = st.lastMove;

            Debug.Assert(m.IsOk());

            var to = m.To();
            Debug.Assert(to.IsOk());
            // 盤外(Square.NB)への移動はありえないのでIsOkPlus1()ではなくIsOk()で良い。

            // --- 移動後の駒

            Piece moved_after_pc = PieceOn(to);

            // 移動前の駒
            Piece moved_pc = m.IsPromote() ? (moved_after_pc - (int)Piece.PROMOTE) : moved_after_pc;

            if (m.IsDrop())
            {
                // --- 駒打ち

                // toの場所にある駒を手駒に戻す
                Piece pt = moved_after_pc.RawType();

                var pn = PieceNoOn(to);
                HandPieceNo(us, pt, hands[(int)us].Count(pt)) = pn;

                Hand(us).Add(pt);

                // toの場所から駒を消す
                RemovePiece(to);
                PieceNoOn(to) = PieceNo.NONE;
            }
            else
            {
                // --- 通常の指し手

                var from = m.From();
                Debug.Assert(from.IsOk());

                // toの場所にあった駒番号
                var pn = PieceNoOn(to);

                // toの場所から駒を消す
                RemovePiece(to);

                // toの地点には捕獲された駒があるならその駒が盤面に戻り、手駒から減る。
                // 駒打ちの場合は捕獲された駒があるということはありえない。
                // (なので駒打ちの場合は、st->capturedTypeを設定していないから参照してはならない)
                if (st.capturedPiece != Piece.NO_PIECE)
                {
                    Piece to_pc = st.capturedPiece;
                    Piece pr = to_pc.RawType();

                    // 盤面のtoの地点に捕獲されていた駒を復元する
                    var pn2 = HandPieceNo(us, pr, Hand(us).Count(pr) - 1);
                    PutPiece(to, to_pc, pn2);
                    PutPiece(from, moved_pc, pn);

                    // 手駒から減らす
                    Hand(us).Sub(pr);
                }
                else
                {
                    PutPiece(from, moved_pc, pn);
                    PieceNoOn(to) = PieceNo.NONE;
                }
            }

            // PutPiece()などを呼び出したので更新する。
            UpdateBitboards();

            // --- 相手番に変更
            sideToMove = us; // Usは先後入れ替えて呼び出されているはず。

            // --- StateInfoを巻き戻す
            st = st.previous;

            --gamePly;
        }

        /// <summary>
        /// 指し手が合法か
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public bool IsLegal(Move m)
        {
            Color Us = sideToMove;
            Square to = m.To();
            Piece toPcType;

            if (m.IsDrop())
            {
                Piece pr = toPcType = m.DroppedPiece();

                // 打つ駒は適切か
                if (pr < Piece.PAWN || Piece.KING <= pr)
                    return false;

                // 駒を持っているか
                if (!Hand(Us).Exist(pr))
                    return false;

                // 行き先に駒はないか
                if ((Pieces() & to).IsNotZero())
                    return false;

                if (pr == Piece.PAWN)
                {
                    var rank_1 = (Us == Color.BLACK) ? Rank.RANK_1 : Rank.RANK_5;
                    if (to.ToRank() == rank_1)
                        return false;

                    // 二歩と打ち歩詰めのチェック
                    if (!LegalPawnDrop(Us, to))
                        return false;
                }
            }
            else
            {
                Square from = m.From();
                Piece moved_pc = PieceOn(from);

                if (moved_pc == Piece.NO_PIECE)
                    return false;

                // 手番側の駒か
                if (moved_pc.PieceColor() != sideToMove)
                    return false;

                // 行き先に味方の駒はないか
                if ((Pieces(Us) & to).IsNotZero())  /* to_pc != Piece.NO_PIECE && to_pc.PieceColor() == SideToMove */
                    return false;

                // 駒の動きは適切か
                if ((Bitboard.EffectsFrom(moved_pc, from, Pieces()) & to).IsZero())
                    return false;

                // 成りが適切か
                if (m.IsPromote())
                {
                }

                // 王手している駒があるか
                if (InCheck())
                {
                    if (moved_pc.Type() != Piece.KING)
                    {
                        // 両王手の場合、玉を動かすほかない
                        if (Checkers().PopCount() > 1)
                            return false;

                        if (((Bitboard.BetweenBB(Checkers().Pop(), KingSquare(Us)) | Checkers()) & to).IsZero())
                            return false;
                    }
                }

                // 自殺手
                if (moved_pc.Type() == Piece.KING)
                {
                    // 玉の移動先に相手側の利きがあるか
                    if (EffectedTo(Us.Not(), to, from))
                        return false;
                }
                else
                {
                    var b = (PinnedPieces(Us) & from).IsZero() // ピンされていない駒の移動は自由である
                            || Util.IsAligned(from, to, KingSquare(Us)); // ピンされている方角への移動は合法

                    if (!b)
                        return false;
                }
                toPcType = moved_pc.Type();
            }

            if (InCheck() && toPcType != Piece.KING)
            {
                Bitboard target = Checkers();
                Square checkSq = target.Pop();

                // 王手している駒を1個取り除いて、もうひとつあるということは王手している駒が
                // 2つあったということであり、両王手なので合い利かず。
                if (target.IsNotZero())
                    return false;

                // 王と王手している駒との間の升に駒を打っていない場合、それは王手を回避していることに
                // ならないので、これは非合法手。

                // 王手している駒が1つなら、王手している駒を取る指し手であるか、
                // 遮断する指し手でなければならない

                if (!((Bitboard.BetweenBB(checkSq, KingSquare(Us)) & to).IsNotZero() || checkSq == to))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 連続王手の千日手等で引き分けかどうかを返す
        /// 千日手でなければRepetitionState.NONEが返る。
        /// </summary>
        /// <returns></returns>
        public RepetitionState IsRepetition()
        {
            // 現在の局面と同じhash keyを持つ局面が4回あれば、それは千日手局面であると判定する。

#if false
            // Debug用にこの局面に至るまでのHash値をすべて表示させてみる
            {
                var s = st;
                for(int i = 0; s != null ;++i)
                {
                    Debug.Log($"gamePly = {gamePly-i} → {s.key.Pretty()}");
                    s = s.previous;
                }
            }
#endif

            // n回st.previousを辿るlocal method
            StateInfo prev(StateInfo si, int n)
            {
                for (int i = 0; i < n; ++i)
                {
                    si = si.previous;
                    if (si == null)
                        break;
                }
                return si;
            };

            // 4手かけないと千日手にはならないから、4手前から調べていく。
            StateInfo stp = prev(st, 4);
            // 遡った手数のトータル
            int t = 4;

            // 同一である局面が出現した回数
            int cnt = 0;

            //Console.WriteLine("--Start--");
            //Console.WriteLine(st.key.Pretty());

            while (stp != null)
            {
                //Console.WriteLine(stp.key.Pretty());

                // HashKeyは128bitもあるのでこのチェックで現実的には間違いないだろう。
                if (stp.key == st.key)
                {
                    // 同一局面が4回出現した時点で千日手が成立
                    if (++cnt == 3)
                    {
                        // 自分が王手をしている連続王手の千日手なのか？
                        if (t <= st.continuousCheck[(int)sideToMove])
                            return RepetitionState.LOSE;

                        // 相手が王手をしている連続王手の千日手なのか？
                        if (t <= st.continuousCheck[(int)sideToMove.Not()])
                            return RepetitionState.WIN;

                        if (sideToMove == Color.BLACK)
                            return RepetitionState.LOSE;

                        if (sideToMove == Color.WHITE)
                            return RepetitionState.WIN;

                        //return RepetitionState.DRAW;
                        Debug.Assert(false);
                    }
                }
                // ここから2手ずつ遡る
                stp = prev(stp, 2);
                t += 2;
            }

            // 同じhash keyの局面が見つからなかったので…。
            return RepetitionState.NONE;
        }

        /// <summary>
        /// この局面で手番側が詰んでいるか(合法な指し手がないか)
        /// 実際に指し手生成をして判定を行うので、引数として指し手生成バッファを渡してやる必要がある。
        /// </summary>
        /// <returns></returns>
        public bool IsMated(Move[] moves)
        {
            return InCheck() && MoveGen.LegalAll(this, moves, 0) == 0;
        }


        /// <summary>
        /// c側の手駒の参照
        /// </summary>
        public ref Hand Hand(Color c) { return ref hands[(int)c]; }

        /// <summary>
        /// c側の玉のSquareへの参照
        /// </summary>
        public ref Square KingSquare(Color c) { return ref kingSquare[(int)c]; }

        /// <summary>
        /// 盤面上sqにある駒の参照
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public ref Piece PieceOn(Square sq) { return ref board[(int)sq]; }

        /// <summary>
        /// 盤面上と駒台sqにある駒。
        /// 何もないときはNO_PIECEが返る。
        /// </summary>
        /// <param name="sq"></param>
        /// <returns></returns>
        public Piece PieceOn(SquareHand sq)
        {
            if (sq.IsBoardPiece())
                return board[(int)sq];
            else
            {
                Color c = sq.PieceColor();
                Piece pr = sq.ToPiece();
                if (Hand(c).Count(pr) >= 1)
                    return Util.MakePiece(c, pr);
            }
            return Piece.NO_PIECE;
        }

        public ref PieceNo PieceNoOn(Square sq) { return ref board_pn[(int)sq]; }

        public ref PieceNo HandPieceNo(Color c, Piece pt, int no) { return ref hand_pn[(int)c, (int)pt, no]; }

        /// <summary>
        /// 現局面で王手がかかっているか
        /// </summary>
        /// <returns></returns>
        public bool InCheck() { return Checkers().IsNotZero(); }

        /// <summary>
        /// 合法な打ち歩か。
        /// 二歩でなく、かつ打ち歩詰めでないならtrueを返す。
        /// </summary>
        /// <param name="us"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool LegalPawnDrop(Color us, Square to)
        {
            return !(((Pieces(us, Piece.PAWN) & Bitboard.FileBB(to.ToFile())).IsNotZero())                   // 二歩
                || ((Bitboard.PawnEffect(us, to) == new Bitboard(KingSquare(us.Not())) && !LegalDrop(to)))); // 打ち歩詰め
        }

        /// <summary>
        /// toの地点に歩を打ったときに打ち歩詰めにならないならtrue。
        /// 歩をtoに打つことと、二歩でないこと、toの前に敵玉がいることまでは確定しているものとする。
        /// 二歩の判定もしたいなら、legal_pawn_drop()のほうを使ったほうがいい。
        /// </summary>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool LegalDrop(Square to)
        {
            var us = sideToMove;

            // 打とうとする歩の利きに相手玉がいることは前提条件としてクリアしているはず。
            // ASSERT_LV3(pawnEffect(us, to) == Bitboard(king_square(~us)));

            // この歩に利いている自駒(歩を打つほうの駒)がなければ詰みには程遠いのでtrue
            if (!EffectedTo(us, to))
                return true;

            // ここに利いている敵の駒があり、その駒で取れるなら打ち歩詰めではない
            // ここでは玉は除外されるし、香が利いていることもないし、そういう意味では、特化した関数が必要。
            Bitboard b = AttackersToPawn(us.Not(), to);

            // このpinnedは敵のpinned pieces
            Bitboard pinned = PinnedPieces(us.Not());

            // pinされていない駒が1つでもあるなら、相手はその駒で取って何事もない。
            if ((b & (~pinned | Bitboard.FileBB(to.ToFile()))).IsNotZero())
                return true;

            // 攻撃駒はすべてpinされていたということであり、
            // 王の頭に打たれた打ち歩をpinされている駒で取れるケースは、
            // いろいろあるが、例1),例2)のような場合であるから、例3)のケースを除き、
            // いずれも玉の頭方向以外のところからの玉頭方向への移動であるから、
            // pinされている方向への移動ということはありえない。
            // 例3)のケースを除くと、この歩は取れないことが確定する。
            // 例3)のケースを除外するために同じ筋のものはpinされていないものとして扱う。
            //    上のコードの　 " | FILE_BB[file_of(to)] " の部分がそれ。

            // 例1)
            // ^玉 ^角  飛
            //  歩

            // 例2)
            // ^玉
            //  歩 ^飛
            //          角

            // 例3)
            // ^玉
            //  歩
            // ^飛
            //  香

            // 玉の退路を探す
            // 自駒がなくて、かつ、to(はすでに調べたので)以外の地点

            // 相手玉の場所
            Square sq_king = KingSquare(us.Not());

            // LONG EFFECT LIBRARYがない場合、愚直に8方向のうち逃げられそうな場所を探すしかない。

            Bitboard escape_bb = Bitboard.KingEffect(sq_king) & ~Pieces(us.Not());
            escape_bb ^= to;
            var occ = Pieces() ^ to; // toには歩をおく前提なので、ここには駒があるものとして、これでの利きの遮断は考えないといけない。
            while (escape_bb.IsNotZero())
            {
                Square king_to = escape_bb.Pop();
                if (AttackersTo(us, king_to, occ).IsZero())
                    return true; // 退路が見つかったので打ち歩詰めではない。
            }

            // すべての検査を抜けてきたのでこれは打ち歩詰めの条件を満たしている。
            return false;
        }

        // -------------------------------------------------------------------------
        //  Bitboard
        // -------------------------------------------------------------------------

        public Bitboard Pieces() { return byTypeBB[(int)Piece.ALL]; }
        public Bitboard Pieces(Color c) { return byColorBB[(int)c]; }
        public Bitboard Pieces(Color c, Piece pc) { return Pieces(c) & Pieces(pc); }

        /// <summary>
        /// 駒がある升を表すBitboardが返る
        /// GOLDS, HDKなど特殊な定数も使用可
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Bitboard Pieces(Piece pt) { return byTypeBB[(int)pt]; }
        public Bitboard Pieces(Piece pc1, Piece pc2) { return Pieces(pc1) | Pieces(pc2); }
        public Bitboard Pieces(Piece pc1, Piece pc2, Piece pc3) { return Pieces(pc1) | Pieces(pc2) | Pieces(pc3); }
        public Bitboard Pieces(Piece pc1, Piece pc2, Piece pc3, Piece pc4) { return Pieces(pc1) | Pieces(pc2) | Pieces(pc3) | Pieces(pc4); }
        public Bitboard Pieces(Piece pc1, Piece pc2, Piece pc3, Piece pc4, Piece pc5) { return Pieces(pc1) | Pieces(pc2) | Pieces(pc3) | Pieces(pc4) | Pieces(pc5); }

        /// <summary>
        /// 駒がない升が1になっているBitboardが返る
        /// </summary>
        /// <returns></returns>
        public Bitboard Empties() { return Pieces() ^ Bitboard.AllBB(); }

        /// <summary>
        /// 現局面で王手している駒のbitboard
        /// </summary>
        /// <returns></returns>
        public Bitboard Checkers() { return st.checkersBB; }
        public Bitboard PinnedPieces(Color c) { return st.blockersForKing[(int)c] & Pieces(c); }
        public Bitboard CheckSquares(Piece pc) { return st.checkSquares[(int)pc]; }

        // 利き
        /// <summary>
        /// 現局面でsqに利いているc側の駒を列挙する
        /// </summary>
        /// <param name="st"></param>
        public Bitboard AttackersTo(Color c, Square sq) { return AttackersTo(c, sq, Pieces()); }

        public Bitboard AttackersTo(Color c, Square sq, Bitboard occ)
        {
            Color them = c.Not();

            // sqの地点に敵駒ptをおいて、その利きに自駒のptがあればsqに利いている
            return
                ( (Bitboard.PawnEffect(them, sq)   & Pieces(Piece.PAWN))
                | (Bitboard.SilverEffect(them, sq) & Pieces(Piece.SILVER_HDK))
                | (Bitboard.GoldEffect(them, sq)   & Pieces(Piece.GOLDS_HDK))
                | (Bitboard.BishopEffect(sq, occ)  & Pieces(Piece.BISHOP_HORSE))
                | (Bitboard.RookEffect(sq, occ)    & Pieces(Piece.ROOK_DRAGON))
                ) & Pieces(c);
        }

        /// <summary>
        /// 現局面でsqに利いている駒を列挙する
        /// </summary>
        /// <param name="st"></param>
        public Bitboard AttackersTo(Square sq) { return AttackersTo(sq, Pieces()); }

        public Bitboard AttackersTo(Square sq, Bitboard occ)
        {
            // sqの地点に敵駒ptをおいて、その利きに自駒のptがあればsqに利いている
            return
                // 先手の歩・銀・金・HDK
                (( (Bitboard.PawnEffect(Color.WHITE, sq)   & Pieces(Piece.PAWN))
                 | (Bitboard.SilverEffect(Color.WHITE, sq) & Pieces(Piece.SILVER_HDK))
                 | (Bitboard.GoldEffect(Color.WHITE, sq)   & Pieces(Piece.GOLDS_HDK))
                 ) & Pieces(Color.BLACK))
                |
                // 後手の歩・銀・金・HDK
                (( (Bitboard.PawnEffect(Color.BLACK, sq)   & Pieces(Piece.PAWN))
                 | (Bitboard.SilverEffect(Color.BLACK, sq) & Pieces(Piece.SILVER_HDK))
                 | (Bitboard.GoldEffect(Color.BLACK, sq)   & Pieces(Piece.GOLDS_HDK))
                 ) & Pieces(Color.WHITE))

                // 先後の角・飛
                | (Bitboard.BishopEffect(sq, occ) & Pieces(Piece.BISHOP_HORSE))
                | (Bitboard.RookEffect(sq, occ)   & Pieces(Piece.ROOK_DRAGON));
        }

        /// <summary>
        /// 打ち歩詰め判定に使う。王に打ち歩された歩の升をpawn_sqとして、c側(王側)のpawn_sqへ利いている駒を列挙する。香が利いていないことは自明。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="pawn_sq"></param>
        /// <returns></returns>
        public Bitboard AttackersToPawn(Color c, Square pawn_sq)
        {
            //    ASSERT_LV3(is_ok(c) && pawn_sq <= SQ_NB);

            Color them = c.Not();
            Bitboard occ = Pieces();

            // 馬と龍
            Bitboard bb_hd = Bitboard.KingEffect(pawn_sq) & Pieces(Piece.HORSE, Piece.DRAGON);
            // 馬、龍の利きは考慮しないといけない。しかしここに玉が含まれるので玉は取り除く必要がある。
            // bb_hdは銀と金のところに加えてしまうことでテーブル参照を一回減らす。

            // sの地点に敵駒ptをおいて、その利きに自駒のptがあればsに利いているということだ。
            // 打ち歩詰め判定なので、その打たれた歩を歩、香、王で取れることはない。(王で取れないことは事前にチェック済)
            return
                ((Bitboard.KnightEffect(them, pawn_sq) & Pieces(Piece.KNIGHT))
                    | (Bitboard.SilverEffect(them, pawn_sq) & (Pieces(Piece.SILVER) | bb_hd))
                    | (Bitboard.GoldEffect(them, pawn_sq) & (Pieces(Piece.GOLDS) | bb_hd))
                    | (Bitboard.BishopEffect(pawn_sq, occ) & Pieces(Piece.BISHOP_HORSE))
                    | (Bitboard.RookEffect(pawn_sq, occ) & Pieces(Piece.ROOK_DRAGON))
                    ) & Pieces(c);
        }

        /// <summary>
        /// attackers_to()のbool版
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <returns></returns>
        public bool EffectedTo(Color c, Square sq) { return AttackersTo(c, sq, Pieces()).IsNotZero(); }

        /// <summary>
        /// kingSqの地点からは玉を取り除いての利きの判定を行なう。
        /// </summary>
        public bool EffectedTo(Color c, Square sq, Square kingSq) { return AttackersTo(c, sq, Pieces() ^ kingSq).IsNotZero(); }

        // -------------------------------------------------------------------------
        //  private methods
        // -------------------------------------------------------------------------

        /// <summary>
        /// StateInfoの値を初期化する。
        /// やねうら王から移植
        /// </summary>
        /// <param name="si"></param>
        private void SetState(StateInfo st)
        {
            // --- bitboard

            // この局面で自玉に王手している敵駒
            //st->checkersBB = attackers_to(~sideToMove, king_square(sideToMove));

            // --- hash keyの計算
            st.key = sideToMove == Color.BLACK ? Zobrist.Zero : Zobrist.Side;
            foreach (var sq in All.Squares())
            {
                var pc = PieceOn(sq);
                st.key += Zobrist.Psq(sq, pc);
            }
            foreach (var c in All.Colors())
                for (Piece pr = Piece.PAWN; pr < Piece.HAND_NB; ++pr)
                    st.key += Zobrist.Hand(c, pr) * Hand(c).Count(pr); // 手駒はaddにする(差分計算が楽になるため)
        }

        private void SetCheckInfo(StateInfo st)
        {
            st.checkersBB = AttackersTo(sideToMove.Not(), KingSquare(sideToMove));

            st.blockersForKing[(int)Color.BLACK] = SliderBlockers(Color.WHITE, KingSquare(Color.BLACK), st.pinnersForKing[(int)Color.BLACK]);
            st.blockersForKing[(int)Color.WHITE] = SliderBlockers(Color.BLACK, KingSquare(Color.WHITE), st.pinnersForKing[(int)Color.WHITE]);

            Color them = sideToMove.Not();
            Square ksq = KingSquare(them);
            Bitboard occ = Pieces();

            st.checkSquares[(int)Piece.PAWN]   = Bitboard.PawnEffect(them, ksq);
            st.checkSquares[(int)Piece.SILVER] = Bitboard.SilverEffect(them, ksq);
            st.checkSquares[(int)Piece.BISHOP] = Bitboard.BishopEffect(ksq, occ);
            st.checkSquares[(int)Piece.ROOK]   = Bitboard.RookEffect(ksq, occ);
            st.checkSquares[(int)Piece.GOLD]   = Bitboard.GoldEffect(them, ksq);
            st.checkSquares[(int)Piece.KING]   = Bitboard.ZeroBB();

            st.checkSquares[(int)Piece.PRO_PAWN]   = st.checkSquares[(int)Piece.GOLD];
            st.checkSquares[(int)Piece.PRO_SILVER] = st.checkSquares[(int)Piece.GOLD];
            st.checkSquares[(int)Piece.HORSE]      = st.checkSquares[(int)Piece.BISHOP] | Bitboard.KingEffect(ksq);
            st.checkSquares[(int)Piece.DRAGON]     = st.checkSquares[(int)Piece.ROOK]   | Bitboard.KingEffect(ksq);
        
            if(st.previous != null)
            {
                Color Us = them.Not();
                st.continuousCheck[(int)them] = st.checkersBB.IsNotZero() ? st.previous.continuousCheck[(int)them] + 2 : 0;
                st.continuousCheck[(int)Us] = st.previous.continuousCheck[(int)Us];
            }
        }

        private void PutPiece(Square sq, Piece pc, PieceNo pn)
        {
            Debug.Assert(PieceOn(sq) == Piece.NO_PIECE);

            PieceOn(sq) = pc;
            PieceNoOn(sq) = pn;

            // 玉であれば、KingSquareを更新する
            if (pc.Type() == Piece.KING)
                KingSquare(pc.PieceColor()) = sq;

            XorPiece(sq, pc);
        }

        private Piece RemovePiece(Square sq)
        {
            var pc = board[(int)sq];
            Debug.Assert(pc != Piece.NO_PIECE);
            board[(int)sq] = Piece.NO_PIECE;

            // 玉であれば、KingSquareを更新する
            if (pc.Type() == Piece.KING)
                KingSquare(pc.PieceColor()) = Square.NB;

            XorPiece(sq, pc);

            return pc;
        }

        private void XorPiece(Square sq, Piece pc)
        {
            byColorBB[(int)pc.PieceColor()] ^= sq;
            byTypeBB[(int)Piece.ALL] ^= sq;
            byTypeBB[(int)pc.Type()] ^= sq;
        }

        /// <summary>
        /// PutPiece(), RemovePiece(), XorPiece()を用いたあとに呼び出す必要がある。
        /// </summary>
        void UpdateBitboards()
        {
            // 王・馬・龍を合成
            byTypeBB[(int)Piece.HDK] = Pieces(Piece.KING, Piece.HORSE, Piece.DRAGON);

            // 金と同じ移動特性を持つ駒
            byTypeBB[(int)Piece.GOLDS] = Pieces(Piece.GOLD, Piece.PRO_PAWN, Piece.PRO_SILVER);

            // 角と馬
            byTypeBB[(int)Piece.BISHOP_HORSE] = Pieces(Piece.BISHOP, Piece.HORSE);

            // 飛と龍
            byTypeBB[(int)Piece.ROOK_DRAGON] = Pieces(Piece.ROOK, Piece.DRAGON);

            // 銀とHDK
            byTypeBB[(int)Piece.SILVER_HDK] = Pieces(Piece.SILVER, Piece.HDK);

            // 金相当の駒とHDK
            byTypeBB[(int)Piece.GOLDS_HDK] = Pieces(Piece.GOLDS, Piece.HDK);
        }

        /// <summary>
        /// 升sに対して、c側の大駒に含まれる長い利きを持つ駒の利きを遮っている駒のBitboardを返す(先後の区別なし)
        /// ※　Stockfishでは、sildersを渡すようになっているが、大駒のcolorを渡す実装のほうが優れているので変更。
        /// [Out] pinnersとは、pinされている駒が取り除かれたときに升sに利きが発生する大駒である。これは返し値。
        /// また、升sにある玉は~c側のKINGであるとする。
        /// </summary>
        /// <param name="c"></param>
        /// <param name="sq"></param>
        /// <param name="pinners"></param>
        /// <returns></returns>
        public Bitboard SliderBlockers(Color c, Square sq, Bitboard pinners)
        {
            Bitboard result = Bitboard.ZeroBB();

            pinners = Bitboard.ZeroBB();

            Bitboard snipers =
                ((Pieces(Piece.ROOK_DRAGON) & Bitboard.RookStepEffect(sq))
                | (Pieces(Piece.BISHOP_HORSE) & Bitboard.BishopStepEffect(sq))
                ) & Pieces(c);

            while (snipers.IsNotZero())
            {
                Square sniperSq = snipers.Pop();
                Bitboard b = Bitboard.BetweenBB(sq, sniperSq) & Pieces();

                // snipperと玉との間にある駒が1個であるなら。
                // (間にある駒が0個の場合、b == ZERO_BBとなり、何も変化しない。)
                if (!Bitboard.MoreThanOne(b))
                {
                    result |= b;
                    if ((b & Pieces(c.Not())).IsNotZero())
                        // sniperと玉に挟まれた駒が玉と同じ色の駒であるなら、pinnerに追加。
                        pinners |= sniperSq;
                }
            }
            return result;
        }

    }
};