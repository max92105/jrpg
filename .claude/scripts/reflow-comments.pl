# Rewraps to 120 columns the /// <summary>, <returns>, <remarks>, <param> and <exception> blocks and the // blocks of C#
# files that changed since HEAD, keeping CRLF; a new one-line /// <summary>, <returns>, <param> or <exception> past 120
# columns becomes a block.
# A tag such as <see cref="X"/> is one word, so it is never split. Usage: perl .claude/scripts/reflow-comments.pl file...
use strict; use warnings;
my $width = 120;
for my $file (@ARGV) {
  my $head = `git show HEAD:$file`;
  $head =~ s/\r//g;
  my %old = map { $_ => 1 } split /\n/, $head;
  local $/; open my $in, '<:raw', $file or die "$file: $!"; my $text = <$in>; close $in;
  my @lines = split /\r\n/, $text, -1;
  my @out;
  for (my $i = 0; $i < @lines; $i++) {
    my ($open, $prefix, $body_re, $end);
    if ($lines[$i] =~ /^(\s*)\/\/\/ (<(summary|returns|param|exception)[^>]*>)(.+)(<\/\3>)$/ && length($lines[$i]) > $width && !$old{$lines[$i]}) {
      push @out, "$1/// $2", wrap("$1/// ", $4), "$1/// $5"; next;
    }
    if ($lines[$i] =~ /^(\s*)\/\/\/ <(summary|returns|remarks|param|exception)(?: [^>]*)?>$/) {
      push @out, $lines[$i];
      $prefix = "$1/// "; my $tag = $2;
      my $j = $i + 1; my @body;
      while ($j < @lines && $lines[$j] !~ /^\s*\/\/\/ <\/$tag>$/) { push @body, $lines[$j]; $j++; }
      if (grep { !$old{$_} } @body) { push @out, wrap($prefix, join ' ', map { (my $l = $_) =~ s/^\s*\/\/\/ ?//; $l } @body); }
      else { push @out, @body; }
      push @out, $lines[$j]; $i = $j; next;
    }
    if ($lines[$i] =~ /^(\s*)\/\/ (?!\/)/) {
      my $indent = $1; my $j = $i; my @block;
      while ($j < @lines && $lines[$j] =~ /^\Q$indent\E\/\/ (?!\/)/) { push @block, $lines[$j]; $j++; }
      if (@block > 1 && grep { !$old{$_} } @block) { push @out, wrap("$indent// ", join ' ', map { (my $l = $_) =~ s/^\s*\/\/ //; $l } @block); }
      else { push @out, @block; }
      $i = $j - 1; next;
    }
    push @out, $lines[$i];
  }
  open my $outf, '>:raw', $file or die; print $outf join("\r\n", @out); close $outf;
}
print "ok\n";

sub wrap {
  my ($prefix, $words) = @_;
  my @tokens = $words =~ /(?:<[^>]*>|[^\s<])+/g;
  my (@result, $line);
  $line = '';
  for my $token (@tokens) {
    if ($line ne '' && length($prefix) + length($line) + 1 + length($token) > $width) { push @result, $prefix . $line; $line = $token; }
    else { $line = $line eq '' ? $token : "$line $token"; }
  }
  push @result, $prefix . $line if $line ne '';
  return @result;
}
