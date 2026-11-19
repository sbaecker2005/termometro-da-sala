"""Estimativa de potência dos ocupantes; não calcula temperatura nem previsão ML."""
import json
import math

REFERENCIA = (
    'https://energyplus.readthedocs.io/en/v26.2.0/guides/input-output-reference/'
    '1.14-group-internal-gains-people-lights-other-internal-zone-equipment.html'
)


def estimar_ocupantes(grupos):
    """Soma quantidade × W/pessoa por grupo com referência explicitamente informada.

    Potência ausente em grupo presente deixa o total desconhecido (None).
    Lista vazia significa sala explicitamente vazia; presença desconhecida é inválida.
    """
    if not isinstance(grupos, list):
        raise ValueError('Informe a lista de grupos; [] significa sala vazia conhecida.')
    resultado = []
    pessoas = 0
    total = 0.0
    completo = True
    for grupo in grupos:
        if not isinstance(grupo, dict):
            raise ValueError('Cada grupo deve ser um objeto.')
        for campo in ('grupo', 'perfil_etario', 'atividade'):
            if not isinstance(grupo.get(campo), str) or not grupo[campo].strip():
                raise ValueError(f'Informe {campo} do grupo, sem dados individuais.')
        quantidade = grupo.get('quantidade')
        if type(quantidade) is not int or quantidade < 0:
            raise ValueError('Quantidade deve ser um inteiro não negativo, conhecido.')
        potencia = grupo.get('potencia_por_pessoa_w')
        referencia = grupo.get('referencia')
        if potencia is not None:
            try:
                valida = (type(potencia) in (int, float)
                          and math.isfinite(potencia) and potencia > 0)
            except OverflowError:
                valida = False
            if not valida:
                raise ValueError('Potência por pessoa deve ser positiva e finita ou None.')
            if not isinstance(referencia, str) or not referencia.strip():
                raise ValueError('Uma potência informada exige sua referência e hipótese.')
        try:
            calor = 0.0 if quantidade == 0 else (
                None if potencia is None else quantidade * potencia)
            if calor is None:
                completo = False
            else:
                total += calor
                if not math.isfinite(total):
                    raise ValueError('Potência total excede o intervalo numérico.')
        except OverflowError as exc:
            raise ValueError('Potência total excede o intervalo numérico.') from exc
        pessoas += quantidade
        resultado.append({
            'grupo': grupo['grupo'], 'perfil_etario': grupo['perfil_etario'],
            'atividade': grupo['atividade'], 'quantidade': quantidade,
            'potencia_por_pessoa_w': potencia, 'referencia': referencia,
            'potencia_grupo_w': calor,
        })
    return {
        'origem': 'estimativa', 'pessoas': pessoas, 'completo': completo,
        'potencia_total_w': total if completo else None, 'grupos': resultado,
    }


def exemplo_terceirao():
    """Cenário didático adulto equivalente; não é calibração de adolescentes."""
    hipotese = (f'{REFERENCIA} — tabela 1.30, coluna W/pessoa, adulto de referência '
                'com 1,8 m²; hipótese de demonstração, não medição dos participantes.')
    grupos = [
        {'grupo': 'alunos', 'perfil_etario': '17–18 anos aproximadamente, hipotético',
         'atividade': 'sentados em repouso', 'quantidade': 20,
         'potencia_por_pessoa_w': 108, 'referencia': hipotese},
        {'grupo': 'professor', 'perfil_etario': 'adulto, hipotético',
         'atividade': 'em pé relaxado', 'quantidade': 1,
         'potencia_por_pessoa_w': 126, 'referencia': hipotese},
    ]
    return {
        'cenario': 'Demonstração do 3º ano do ensino médio — 20 alunos e 1 professor',
        'validado_em_campo': False,
        'hipotese': 'Referência adulta equivalente aplicada explicitamente só ao exemplo.',
        'limite': ('Potência total inclui calor sensível e latente. Não representa aumento '
                   'de temperatura e não é entrada do modelo exportado atual.'),
        **estimar_ocupantes(grupos),
    }


if __name__ == '__main__':
    print(json.dumps(exemplo_terceirao(), ensure_ascii=False, indent=2, allow_nan=False))
