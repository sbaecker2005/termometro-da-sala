const horaSp = new Intl.DateTimeFormat('pt-BR', {
  timeZone: 'America/Sao_Paulo',
  day: '2-digit',
  month: '2-digit',
  hour: '2-digit',
  minute: '2-digit',
});

export function horaSaoPaulo(isoUtc: string): string {
  const data = new Date(isoUtc);
  if (Number.isNaN(data.getTime())) return isoUtc;
  return horaSp.format(data);
}

export function indiceEmGraus(valor: number): string {
  return `${valor.toLocaleString('pt-BR', { minimumFractionDigits: 1, maximumFractionDigits: 1 })} °C`;
}
