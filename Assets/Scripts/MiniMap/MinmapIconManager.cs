using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class MinmapIconManager : MonoBehaviour
{
    [SerializeField] GameObject miniMapCamera;
    [SerializeField] GameObject myIcon;
    [SerializeField] GameObject EnemyIcon;
    [Header("Layers")]
    [SerializeField] LayerMask showMinimapLayer;
    [SerializeField] LayerMask hideMinimapLayer;
    private PhotonView pv;


    void Start()
    {
        pv = GetComponent<PhotonView>();

        if (pv.IsMine)
        {
            myIcon.SetActive(true);
            EnemyIcon.SetActive(false);
            miniMapCamera.SetActive(true);

            myIcon.layer = showMinimapLayer;
            EnemyIcon.layer = hideMinimapLayer;

            PlayerMachanics.A_PlayerShooting += PlayerShooting;
        }
        else
        {
            myIcon.layer = hideMinimapLayer;
            EnemyIcon.layer = showMinimapLayer;
            myIcon.SetActive(false);
            EnemyIcon.SetActive(false);
            miniMapCamera.SetActive(false);
        }
    }

    void OnDisable()
    {
        PlayerMachanics.A_PlayerShooting -= PlayerShooting;
    }

    [PunRPC]
    void ShowEnemyIcon(bool show)
    {
        if (!pv.IsMine && show)
        {
            
            EnemyIcon.SetActive(true);
            CancelInvoke(nameof(HideEnemyicon));
            Invoke(nameof(HideEnemyicon), 1f);
        }
        else
        {
            Invoke(nameof(HideEnemyicon), 1f);
        }
    }

    void HideEnemyicon()
    {
        EnemyIcon.SetActive(false);
    }

    

    void PlayerShooting(bool isShooting)
    {
        pv.RPC(nameof(ShowEnemyIcon), RpcTarget.All, isShooting);
    }

}
