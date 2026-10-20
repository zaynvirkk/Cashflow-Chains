#!/usr/bin/env python3
"""Renders markdown specs to accessible, selectable-text PDF using WeasyPrint."""
import pathlib, re
import mistune
from weasyprint import HTML
from xml.sax.saxutils import escape
ROOT=pathlib.Path(__file__).resolve().parent.parent
css='''
@page { size: A4; margin: 19mm 18mm 19mm 18mm; @bottom-left {content:"CODEX PRODUCTION SPEC  |  20 OCT 2026";font:8pt "DejaVu Sans";color:#627084} @bottom-right {content:"PAGE " counter(page) " / " counter(pages);font:8pt "DejaVu Sans";color:#627084} }
*{box-sizing:border-box}body{font-family:"DejaVu Sans",sans-serif;font-size:9.25pt;line-height:1.5;color:#202936;}h1{color:#10293a;font-family:"DejaVu Sans",sans-serif;font-size:23pt;line-height:1.2;margin:0 0 11pt;border-bottom:2pt solid #c48a40;padding-bottom:8pt;page-break-after:avoid}
h2{font-size:14pt;line-height:1.3;color:#102f45;border-bottom:.6pt solid #bdcdd2;padding-bottom:4pt;margin:23pt 0 8pt;page-break-after:avoid}h3{color:#9c6222;font-size:10.6pt;margin:15pt 0 5pt;page-break-after:avoid}h4{font-size:9.7pt;color:#425365;page-break-after:avoid}
p{margin:5pt 0 8pt;orphans:3;widows:3}a{color:#147d8b;text-decoration:none;overflow-wrap:anywhere}strong{color:#142f40}blockquote{color:#3b5061;margin:11pt 0;padding:7pt 11pt;border-left:3pt solid #bb8b4b;background:#f1f6f6}ul,ol{margin:6pt 0 9pt;padding-left:17pt}li{margin-bottom:3pt;orphans:2;widows:2}
table{border-collapse:collapse;width:100%;margin:11pt 0 13pt;font-size:7.6pt;table-layout:auto}thead{display:table-header-group}tr{page-break-inside:avoid}th{background:#163547;color:#fff;border:1px solid #345066;text-align:left;font-weight:700;padding:5pt 5pt}td{border:1px solid #d7e0e3;padding:4.5pt 5pt;vertical-align:top;overflow-wrap:anywhere}tbody tr:nth-child(even){background:#f1f6f8}
code{font-family:"DejaVu Sans Mono",monospace;font-size:.85em;background:#e9f2f4;padding:.5pt 2pt;overflow-wrap:anywhere}pre{background:#112532;color:#dceded;padding:9pt;font:7.2pt/1.45 "DejaVu Sans Mono",monospace;white-space:pre-wrap;overflow-wrap:anywhere;page-break-inside:avoid}pre code{background:transparent;color:inherit;padding:0}
hr{height:0;border:0;border-top:1px solid #d1d8df;margin:18pt 0}.coverline{height:4pt;background:linear-gradient(90deg,#19bba4,#e5b562);margin-bottom:13pt}
.footer-note{font-size:8pt;color:#627084;margin-top:18pt}.disclaimer{background:#f6f5ef;padding:8pt 12pt;border-left:3pt solid #d0a658}
'''
md=mistune.create_markdown(plugins=['table','strikethrough','task_lists','url'])
for name in ['01_CASHFLOW_CHAINS_MASTER_SPEC','02_BLACK_BAG_MASTER_SPEC','03_SHARED_TECH_MONETIZATION_LAUNCH']:
 source=ROOT/(name+'.md');raw=source.read_text()
 htmlbody=md(raw)
 html=f'''<!doctype html><html lang="en"><head><meta charset="utf-8"><style>{css}</style><title>{escape(name)}</title></head><body><div class="coverline"></div>{htmlbody}</body></html>'''
 dest=ROOT/(name+'.pdf')
 HTML(string=html,base_url=str(ROOT)).write_pdf(str(dest))
 print(dest,dest.stat().st_size)
