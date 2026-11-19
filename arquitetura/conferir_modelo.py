"""Confere o modelo ArchiMate nos dois arquivos: termometro-da-sala.xml (Open Exchange) e termometro-da-sala.archimate (Archi).

Para cada arquivo: XML bem formado, ids únicos, toda relação com origem e destino existentes,
todo nó de visão apontando para elemento existente, toda ligação de visão apontando para relação
existente entre os nós certos e todo elemento com documentation. Entre os dois: mesmos elementos,
mesmas relações e mesmas visões. Se o Archi estiver instalado, confere também se cada relação é
permitida pela tabela da especificação ArchiMate que acompanha a ferramenta (relationships.xml).

Uso: python arquitetura/conferir_modelo.py            (sai com código 1 se achar erro)
     python arquitetura/conferir_modelo.py --autoteste
"""
import glob
import os
import sys
import xml.etree.ElementTree as ET

PASTA = os.path.dirname(os.path.abspath(__file__))
OEF = "{http://www.opengroup.org/xsd/archimate/3.0/}"
XSI = "{http://www.w3.org/2001/XMLSchema-instance}type"
REGRAS_ARCHI = os.environ.get("ARCHI_RELATIONSHIPS") or next(iter(glob.glob(
    r"C:\Program Files\Archi\plugins\com.archimatetool.model_*\model\relationships.xml")), "")
LETRA = {"a": "Access", "c": "Composition", "f": "Flow", "g": "Aggregation", "i": "Assignment",
         "n": "Influence", "o": "Association", "r": "Realization", "s": "Specialization",
         "t": "Triggering", "v": "Serving"}


def ler_oef(texto):
    """Devolve (ids, elementos, relacoes, visoes) do Open Exchange.

    elementos: {id: (tipo, nome, tem_doc)}; relacoes: {id: (tipo, origem, destino)};
    visoes: {id: (nome, {no: elemento}, {ligacao: (relacao, no_origem, no_destino)})}.
    """
    raiz = ET.fromstring(texto)
    ids = [e.get("identifier") for e in raiz.iter() if e.get("identifier")]
    elementos = {}
    for e in raiz.iter(OEF + "element"):
        doc = e.find(OEF + "documentation")
        elementos[e.get("identifier")] = (e.get(XSI), e.findtext(OEF + "name"), bool(doc is not None and (doc.text or "").strip()))
    relacoes = {r.get("identifier"): (r.get(XSI), r.get("source"), r.get("target")) for r in raiz.iter(OEF + "relationship")}
    visoes = {}
    for v in raiz.iter(OEF + "view"):
        nos = {n.get("identifier"): n.get("elementRef") for n in v.iter(OEF + "node") if n.get("elementRef")}
        nos.update({n.get("identifier"): None for n in v.iter(OEF + "node") if not n.get("elementRef")})  # grupos e notas
        ligacoes = {c.get("identifier"): (c.get("relationshipRef"), c.get("source"), c.get("target")) for c in v.iter(OEF + "connection")}
        visoes[v.get("identifier")] = (v.findtext(OEF + "name"), nos, ligacoes)
    return ids, elementos, relacoes, visoes


def ler_archi(texto):
    """Mesmo formato de ler_oef, a partir do .archimate do Archi."""
    raiz = ET.fromstring(texto)
    ids = [e.get("id") for e in raiz.iter() if e.get("id")]
    elementos, relacoes, visoes = {}, {}, {}
    for e in raiz.iter("element"):
        tipo = (e.get(XSI) or "").replace("archimate:", "")
        if tipo == "ArchimateDiagramModel":
            nos = {c.get("id"): c.get("archimateElement") for c in e.iter("child")}
            ligacoes = {s.get("id"): (s.get("archimateRelationship"), s.get("source"), s.get("target")) for s in e.iter("sourceConnection")}
            visoes[e.get("id")] = (e.get("name"), nos, ligacoes)
        elif tipo.endswith("Relationship"):
            relacoes[e.get("id")] = (tipo[: -len("Relationship")], e.get("source"), e.get("target"))
        else:
            elementos[e.get("id")] = (tipo, e.get("name"), bool((e.findtext("documentation") or "").strip()))
    return ids, elementos, relacoes, visoes


def conferir(nome, modelo):
    """Lista de erros de um arquivo já lido."""
    ids, elementos, relacoes, visoes = modelo
    erros = []
    vistos = set()
    for i in ids:
        if i in vistos:
            erros.append(f"{nome}: id repetido {i}")
        vistos.add(i)
    for rid, (tipo, origem, destino) in relacoes.items():
        for ponta in (origem, destino):
            if ponta not in elementos and ponta not in relacoes:
                erros.append(f"{nome}: relação {rid} ({tipo}) aponta para {ponta}, que não existe")
    for eid, (tipo, nome_el, tem_doc) in elementos.items():
        if not tem_doc:
            erros.append(f"{nome}: elemento {eid} ({nome_el}) sem documentation")
    for vid, (_, nos, ligacoes) in visoes.items():
        for no, eid in nos.items():
            if eid is not None and eid not in elementos:
                erros.append(f"{nome}: visão {vid}, nó {no} aponta para {eid}, que não existe")
        for lig, (rid, no_o, no_d) in ligacoes.items():
            if rid not in relacoes:
                erros.append(f"{nome}: visão {vid}, ligação {lig} aponta para a relação {rid}, que não existe")
                continue
            if nos.get(no_o) != relacoes[rid][1] or nos.get(no_d) != relacoes[rid][2]:
                erros.append(f"{nome}: visão {vid}, ligação {lig} não liga os elementos da relação {rid}")
    return erros


def comparar(a, b):
    """Os dois arquivos precisam ter os mesmos elementos, relações e visões."""
    _, el_a, rel_a, vis_a = a
    _, el_b, rel_b, vis_b = b
    erros = []
    for rotulo, x, y in (("elementos", {k: v[:2] for k, v in el_a.items()}, {k: v[:2] for k, v in el_b.items()}),
                         ("relações", rel_a, rel_b),
                         ("visões", {k: (v[0], v[1], set(v[2])) for k, v in vis_a.items()},
                          {k: (v[0], v[1], set(v[2])) for k, v in vis_b.items()})):
        for k in sorted(set(x) | set(y)):
            if x.get(k) != y.get(k):
                erros.append(f".xml e .archimate divergem em {rotulo}: {k}")
    return erros


def carregar_regras(caminho):
    """{(tipo_origem, tipo_destino): {tipos de relação permitidos}} a partir do relationships.xml do Archi."""
    regras = {}
    for s in ET.parse(caminho).getroot().iter("source"):
        for t in s.iter("target"):
            regras[(s.get("concept"), t.get("concept"))] = {LETRA[c] for c in t.get("relations", "")}
    return regras


def conferir_regras(nome, modelo, regras):
    _, elementos, relacoes, _ = modelo
    erros = []
    for rid, (tipo, origem, destino) in relacoes.items():
        if origem in elementos and destino in elementos:
            par = (elementos[origem][0], elementos[destino][0])
            if tipo not in regras.get(par, set()):
                erros.append(f"{nome}: relação {rid} {tipo} de {par[0]} para {par[1]} não é permitida no ArchiMate")
    return erros


def principal():
    sys.stdout.reconfigure(encoding="utf-8")
    with open(os.path.join(PASTA, "termometro-da-sala.xml"), encoding="utf-8") as f:
        oef = ler_oef(f.read())
    with open(os.path.join(PASTA, "termometro-da-sala.archimate"), encoding="utf-8") as f:
        archi = ler_archi(f.read())
    erros = conferir(".xml", oef) + conferir(".archimate", archi) + comparar(oef, archi)
    if REGRAS_ARCHI and os.path.exists(REGRAS_ARCHI):
        regras = carregar_regras(REGRAS_ARCHI)
        erros += conferir_regras(".xml", oef, regras)
        print(f"Regras de relação: {REGRAS_ARCHI}")
    else:
        print("Regras de relação: Archi não encontrado, conferência das regras pulada")
    _, el, rel, vis = oef
    print(f"Elementos: {len(el)}  Relações: {len(rel)}  Visões: {len(vis)}")
    for vid, (nome_v, nos, ligacoes) in vis.items():
        print(f"  {nome_v}: {sum(1 for e in nos.values() if e)} elementos, {len(ligacoes)} ligações")
    for e in erros:
        print("ERRO:", e)
    print("OK: modelo válido nos dois arquivos" if not erros else f"{len(erros)} erro(s)")
    return 1 if erros else 0


def autoteste():
    """Cada defeito plantado num modelo mínimo precisa aparecer na lista de erros."""
    sys.stdout.reconfigure(encoding="utf-8")
    bom = ('<model xmlns="http://www.opengroup.org/xsd/archimate/3.0/" '
           'xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" identifier="m">'
           '<elements><element identifier="a" xsi:type="BusinessActor"><name>A</name><documentation>d</documentation></element>'
           '<element identifier="b" xsi:type="BusinessRole"><name>B</name><documentation>d</documentation></element></elements>'
           '<relationships><relationship identifier="r" xsi:type="Assignment" source="a" target="b"/></relationships>'
           '<views><diagrams><view identifier="v" xsi:type="Diagram"><name>V</name>'
           '<node identifier="na" elementRef="a" xsi:type="Element"/><node identifier="nb" elementRef="b" xsi:type="Element"/>'
           '<connection identifier="c" relationshipRef="r" xsi:type="Relationship" source="na" target="nb"/>'
           '</view></diagrams></views></model>')
    assert conferir("t", ler_oef(bom)) == []
    regras = {("BusinessActor", "BusinessRole"): {"Assignment"}}
    assert conferir_regras("t", ler_oef(bom), regras) == []
    defeitos = {
        "id repetido": bom.replace('identifier="b"', 'identifier="a"', 1),
        "não existe": bom.replace('target="b"', 'target="x"'),
        "sem documentation": bom.replace("<documentation>d</documentation>", "", 1),
        "nó na aponta": bom.replace('elementRef="a"', 'elementRef="x"'),
        "relação x, que": bom.replace('relationshipRef="r"', 'relationshipRef="x"'),
        "não liga": bom.replace('source="na" target="nb"', 'source="nb" target="na"'),
    }
    for trecho, texto in defeitos.items():
        erros = conferir("t", ler_oef(texto))
        assert any(trecho in e for e in erros), (trecho, erros)
    assert conferir_regras("t", ler_oef(bom.replace('"Assignment"', '"Serving"')), regras)
    try:
        ler_oef(bom[:-5])
        raise AssertionError("XML quebrado passou")
    except ET.ParseError:
        pass
    archi_bom = ('<archimate:model xmlns:archimate="http://www.archimatetool.com/archimate" '
                 'xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" name="m" id="m"><folder name="F" id="f" type="business">'
                 '<element xsi:type="archimate:BusinessActor" name="A" id="a"><documentation>d</documentation></element>'
                 '<element xsi:type="archimate:BusinessRole" name="B" id="b"><documentation>d</documentation></element>'
                 '<element xsi:type="archimate:AssignmentRelationship" id="r" source="a" target="b"/>'
                 '<element xsi:type="archimate:ArchimateDiagramModel" name="V" id="v">'
                 '<child xsi:type="archimate:DiagramObject" id="na" archimateElement="a">'
                 '<sourceConnection xsi:type="archimate:Connection" id="c" source="na" target="nb" archimateRelationship="r"/></child>'
                 '<child xsi:type="archimate:DiagramObject" id="nb" archimateElement="b"/>'
                 '</element></folder></archimate:model>')
    assert conferir("t", ler_archi(archi_bom)) == [] and comparar(ler_oef(bom), ler_archi(archi_bom)) == []
    assert any("divergem" in e for e in comparar(ler_oef(bom), ler_archi(archi_bom.replace('name="B"', 'name="C"'))))
    print(f"Autoteste: {len(defeitos) + 6} casos OK")
    return 0


if __name__ == "__main__":
    sys.exit(autoteste() if "--autoteste" in sys.argv else principal())
