"""Vetor público de demonstração; não usar esta chave em aparelho real."""
import hashlib
import hmac


if __name__ == "__main__":
    assinatura = hmac.new(
        b"segredo-de-teste-sala-03",
        b"sala-03|esp32-sala-03|33.4|58|1794331200",
        hashlib.sha256,
    ).hexdigest()
    assert assinatura == "2f7c847dc59835d4a9bacbf9f4b642af0cc563cbf1bf90efb91d127f38297ba0"
    print(assinatura)
