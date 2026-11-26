using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WellWithinWell.Rules;

namespace WellWithinWell.Presentation
{
    public sealed class CellView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image hit;
        [SerializeField] private Image piece;
        [SerializeField] private Image lastMove;
        [SerializeField] private Text number;
        private BoardSkin skin;
        private BoardPosition position;
        private Action<BoardPosition> onPlay;
        private Action<BoardPosition?> onHover;
        private bool legal;
        private bool hovered;
        private Mark mark;
        private Mark turn;
        private Coroutine drop;

        public void Bind(BoardPosition address, BoardSkin boardSkin, Action<BoardPosition> play,
            Action<BoardPosition?> hover)
        {
            position = address;
            skin = boardSkin;
            onPlay = play;
            onHover = hover;
            hit.raycastTarget = play != null;
            number.text = (position.Cell + 1).ToString();
            lastMove.sprite = skin.LastMove;
        }

        public void Present(Mark value, Mark activeMark, bool available, bool mostRecent, bool animate, bool closed)
        {
            mark = value;
            turn = activeMark;
            legal = available;
            lastMove.enabled = mostRecent;
            number.enabled = value == Mark.Empty && !closed;
            if (!legal)
                hovered = false;
            RefreshPiece();
            if (drop != null)
                StopCoroutine(drop);
            piece.rectTransform.localScale = Vector3.one;
            drop = animate && isActiveAndEnabled ? StartCoroutine(DropPiece()) : null;
        }

        private void RefreshPiece()
        {
            bool preview = legal && hovered && mark == Mark.Empty;
            piece.enabled = mark != Mark.Empty || preview;
            if (piece.enabled)
            {
                piece.sprite = skin.Piece(preview ? turn : mark);
                piece.color = new Color(1, 1, 1, preview ? 0.46f : 1f);
            }
            hit.color = preview ? new Color32(111, 123, 76, 75) : Color.clear;
        }

        private IEnumerator DropPiece()
        {
            const float duration = 0.16f;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                float factor = 1 + 0.24f * Mathf.Pow(1 - t / duration, 3);
                piece.rectTransform.localScale = Vector3.one * factor;
                yield return null;
            }
            piece.rectTransform.localScale = Vector3.one;
            drop = null;
        }

        private void OnDisable()
        {
            hovered = false;
            drop = null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!legal)
                return;
            hovered = true;
            RefreshPiece();
            onHover?.Invoke(position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!hovered)
                return;
            hovered = false;
            RefreshPiece();
            onHover?.Invoke(null);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left && legal)
                onPlay?.Invoke(position);
        }

        public void Wire(Image hitArea, Image token, Image marker, Text label)
        {
            hit = hitArea;
            piece = token;
            lastMove = marker;
            number = label;
        }
    }
}
