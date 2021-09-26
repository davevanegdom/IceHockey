using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cs_ParticleManager : MonoBehaviour
{

    private void SpawnParticle(Vector2 _spawnPos, GameObject _particle)
    {
        GameObject _particleObject = Instantiate(_particle, _spawnPos, Quaternion.identity);
        Destroy(_particleObject, 1f);
    }

    private void OnEnable()
    {
        cs_PlayerController.s_ParticleEffect += SpawnParticle;
        cs_Enemy.s_BloodParticle += SpawnParticle;
    }

    private void OnDisable()
    {
        cs_PlayerController.s_ParticleEffect -= SpawnParticle;
        cs_Enemy.s_BloodParticle -= SpawnParticle;
    }
}
