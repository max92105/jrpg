# Adds, replaces or removes entries of content/audio/sound-sources.json (ADR 0036), reading each sound's name, uploader,
# license and preview from its freesound page, with today's date as `fetched`. Keeps the manifest's layout: entries
# sorted by file, one field per line, CRLF. Then convert with fetch-placeholder-sounds.ps1 -Only <file>.
#
#   perl .claude/scripts/add-sound-source.pl <file>=<id>:<maxSeconds>[:<fadeOutSeconds>[:<gainDb>[:<startSeconds>]]] ...
#   perl .claude/scripts/add-sound-source.pl effects/PlaceholderHit.wav=458867:0.3:0.05
#   perl .claude/scripts/add-sound-source.pl effects/PlaceholderHit.wav=-          removes the entry
#
# The license is read from the page: CC0-1.0, CC-BY-3.0 or CC-BY-4.0, else UNKNOWN, which the fetch script refuses.
use strict; use warnings;
use JSON::PP;

my $path = 'content/audio/sound-sources.json';
die "usage: perl add-sound-source.pl <file>=<id>:<maxSeconds>[:<fadeOut>[:<gainDb>[:<start>]]] ...\n" unless @ARGV;
open my $in, '<:raw', $path or die "$path: $!\n";
my $text = do { local $/; <$in> };
close $in;
my $manifest = JSON::PP->new->decode($text);
my %entries = map { $_->{file} => $_ } @{$manifest->{sounds}};
my @now = localtime;
my $today = sprintf '%04d-%02d-%02d', $now[5] + 1900, $now[4] + 1, $now[3];

for my $spec (@ARGV) {
  my ($file, $rest) = split /=/, $spec, 2;
  die "$spec: expected <file>=<id>:<maxSeconds>...\n" unless defined $rest;
  if ($rest eq '-') { delete $entries{$file}; print "$file removed\n"; next; }
  my ($id, $max, $fade, $gain, $start) = split /:/, $rest;
  my $html = `curl -s -L -A "Mozilla/5.0" "https://freesound.org/s/$id/"`;
  my ($title, $author) = $html =~ m{<title>Freesound - (.*?) by ([^<]*)</title>}s
    or die "$id: no such sound page\n";
  my ($preview) = $html =~ m{(https://cdn\.freesound\.org/previews/[^"' ]*-hq\.mp3)}
    or die "$id: no preview on its page\n";
  my $license = $html =~ /Creative Commons 0/ ? 'CC0-1.0'
    : $html =~ m{licenses/by/4\.0} ? 'CC-BY-4.0' : $html =~ m{licenses/by/3\.0} ? 'CC-BY-3.0' : 'UNKNOWN';
  for ($title, $author) { s/&quot;/"/g; s/&#39;/'/g; s/&amp;/&/g; s/&lt;/</g; s/&gt;/>/g; }
  my %entry = (file => $file, freesoundId => $id + 0, name => $title, author => $author, license => $license,
    page => "https://freesound.org/s/$id/", preview => $preview, fetched => $today, maxSeconds => $max + 0);
  $entry{startSeconds} = $start + 0 if defined $start && $start ne '' && $start != 0;
  $entry{fadeOutSeconds} = $fade + 0 if defined $fade && $fade ne '';
  $entry{gainDb} = $gain + 0 if defined $gain && $gain ne '' && $gain != 0;
  $entries{$file} = \%entry;
  print "$file <- $id \"$title\" by $author, $license\n";
  sleep 2;
}

my $json = JSON::PP->new->allow_nonref;
my @order = qw(file freesoundId name author license page preview fetched maxSeconds startSeconds fadeOutSeconds gainDb);
my @blocks;
for my $file (sort keys %entries) {
  my $entry = $entries{$file};
  my @lines = map { "      \"$_\": " . $json->encode($entry->{$_}) } grep { exists $entry->{$_} } @order;
  push @blocks, "    {\n" . join(",\n", @lines) . "\n    }";
}
my $out = "{\n  \"peakDb\": " . $json->encode($manifest->{peakDb}) . ",\n  \"tunes\": [\n"
  . join(",\n", map { "    " . $json->encode($_) } @{$manifest->{tunes}}) . "\n  ],\n  \"sounds\": [\n"
  . join(",\n", @blocks) . "\n  ]\n}\n";
$out =~ s/\n/\r\n/g;
open my $o, '>:raw', $path or die "$path: $!\n";
print $o $out;
close $o;
