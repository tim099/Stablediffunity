using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UnityChan
{
//
// ↑↓キーでループアニメーションを切り替えるスクリプト（ランダム切り替え付き）Ver.3
// 2014/04/03 N.Kobayashi
//

// Require these components when using this script
	[RequireComponent(typeof(Animator))]



	public class IdleChanger : MonoBehaviour
	{
		public static List<IdleChanger> s_IdleChangers = new List<IdleChanger>();
        private Animator anim;						// Animatorへの参照
		private AnimatorStateInfo currentState;		// 現在のステート状態を保存する参照
		private AnimatorStateInfo previousState;	// ひとつ前のステート状態を保存する参照
		public bool _random = false;				// ランダム判定スタートスイッチ
		public float _threshold = 0.5f;				// ランダム判定の閾値
		public float _interval = 10f;				// ランダム判定のインターバル
		//private float _seed = 0.0f;					// ランダム判定用シード
	


		// Use this for initialization
		void Start ()
		{
			// 各参照の初期化
			anim = GetComponent<Animator> ();
			currentState = anim.GetCurrentAnimatorStateInfo (0);
			previousState = currentState;
			// ランダム判定用関数をスタートする
			StartCoroutine ("RandomChange");
			s_IdleChangers.Add(this);

		}
        private void OnDestroy()
        {
			s_IdleChangers.Remove(this);
        }
        // Update is called once per frame
        void  Update ()
		{
			// ↑キー/スペースが押されたら、ステートを次に送る処理
			if (Input.GetKeyDown ("up") || Input.GetButton ("Jump")) {
				// ブーリアンNextをtrueにする
				anim.SetBool ("Next", true);
			}
		
			// ↓キーが押されたら、ステートを前に戻す処理
			if (Input.GetKeyDown ("down")) {
				// ブーリアンBackをtrueにする
				anim.SetBool ("Back", true);
			}
		
			// "Next"フラグがtrueの時の処理
			if (anim.GetBool ("Next")) {
				// 現在のステートをチェックし、ステート名が違っていたらブーリアンをfalseに戻す
				currentState = anim.GetCurrentAnimatorStateInfo (0);
				if (previousState.nameHash != currentState.nameHash) {
					anim.SetBool ("Next", false);
					previousState = currentState;				
				}
			}
		
			// "Back"フラグがtrueの時の処理
			if (anim.GetBool ("Back")) {
				// 現在のステートをチェックし、ステート名が違っていたらブーリアンをfalseに戻す
				currentState = anim.GetCurrentAnimatorStateInfo (0);
				if (previousState.nameHash != currentState.nameHash) {
					anim.SetBool ("Back", false);
					previousState = currentState;
				}
			}
		}

		public void CustomOnGUI(int iID)
		{
            //GUILayout.Box ("Change Motion");
			using(var aScope = new GUILayout.HorizontalScope("box"))
            {
				GUILayout.Box($"Name: {name}({iID}),Anim:", GUILayout.ExpandWidth(false));
				var aInfos = anim.GetCurrentAnimatorClipInfo(0);
				foreach(var aInfo in aInfos)
                {
					GUILayout.Label(aInfo.clip.name, GUILayout.ExpandWidth(false));
                }
				if (GUILayout.Button("Next Anim"))
                {
					
					anim.SetBool("Next", true);
				}
					
				if (GUILayout.Button("Prev Anim"))
                {
					anim.SetBool("Back", true);
				}
			}

		}


		// ランダム判定用関数
		IEnumerator RandomChange ()
		{
			// 無限ループ開始
			while (true) {
				//ランダム判定スイッチオンの場合
				if (_random) {
					// ランダムシードを取り出し、その大きさによってフラグ設定をする
					float _seed = Random.Range (0.0f, 1.0f);
					if (_seed < _threshold) {
						anim.SetBool ("Back", true);
					} else if (_seed >= _threshold) {
						anim.SetBool ("Next", true);
					}
				}
				// 次の判定までインターバルを置く
				yield return new WaitForSeconds (_interval);
			}

		}

	}
}
